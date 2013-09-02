Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class ToolpathUnfold
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the ToolpathUnfold class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Toolpath Unfold", "Unfold", _
           "Unfolds toolpaths from the 3D Model into the XY Plane", _
           "Sandbox", "Fabrication")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Unfold Origin", "O", "Origin of the unfold operation", GH_ParamAccess.item)
        pManager.AddCurveParameter("Ordered toolpaths", "C", "Ordered Toolpaths", GH_ParamAccess.tree)
        pManager.AddPlaneParameter("Frames", "P", "List of source frames", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddCurveParameter("Unfolded toolpaths", "T", "Unfolded toolpaths", GH_ParamAccess.tree)
        pManager.AddPlaneParameter("Target frames", "F", "Target frames", GH_ParamAccess.tree)
        pManager.AddPointParameter("Grid points", "P", "Grid points", GH_ParamAccess.tree)
        pManager.AddRectangleParameter("Rectangular grid", "R", "Rectangular grid", GH_ParamAccess.tree)
        'pManager.AddNumberParameter("Dimensions", "D", "", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _O As Point3d = Point3d.Unset
        Dim _C As GH_Structure(Of GH_Curve) = Nothing
        Dim _F As GH_Structure(Of GH_Plane) = Nothing

        '2. Retrieve input data.
        If (Not DA.GetData(0, _O)) Then Return
        If (Not DA.GetDataTree(1, _C)) Then Return
        If (Not DA.GetDataTree(2, _F)) Then Return

        '3. Abort on invalid inputs.
        If (Not _O.IsValid) Then Return
        If (_C.IsEmpty) Then Return
        If (_F.IsEmpty) Then Return

        '4. Do something useful.
        '4.1. Get brep box dimensions
        Dim _XY As List(Of Double) = getBrepDimensions(_O, _C, _F)
        Dim _width As Double = _XY.Item(0)
        Dim _height As Double = _XY.Item(1)

        '4.2. Create rectangular grid
        Dim _grid As GH_Structure(Of GH_Rectangle) = createRectangluarGrid(_O, _F, _width, _height)

        '4.3. Create target planes
        Dim _plTree As GH_Structure(Of GH_Plane) = createTargetPlanes(_grid)

        '4.4. Create target planes
        Dim _ptTree As GH_Structure(Of GH_Point) = createGridPoints(_grid)

        '4.5. Reorient curves
        Dim _polyTree As Grasshopper.DataTree(Of PolylineCurve) = reorientCurves(_C, _F, _plTree)

        'DA.SetDataList(0, _XY)
        DA.SetDataTree(0, _polyTree)
        DA.SetDataTree(1, _plTree)
        DA.SetDataTree(2, _ptTree)
        DA.SetDataTree(3, _grid)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Sandbox_Unfold
            'Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{d5b00bf1-fdb5-4a35-8f59-c073de4d2a59}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    Private Function getBrepDimensions(ByVal _Origin As Point3d, ByVal _Curves As GH_Structure(Of GH_Curve), ByVal _Frames As GH_Structure(Of GH_Plane)) As List(Of Double)

        Dim _O As Point3d = _Origin
        Dim _C As GH_Structure(Of GH_Curve) = _Curves
        Dim _F As GH_Structure(Of GH_Plane) = _Frames

        '1. Get brep box dimensions 
        Dim _X As New List(Of Double)
        Dim _Y As New List(Of Double)

        For i As Int32 = 0 To _C.Branches.Count - 1
            Dim _frm As Plane = _F.Branch(i).Item(0).Value
            Dim _crvList As List(Of GH_Curve) = _C.Branch(i)
            Dim _BBox As BoundingBox = Nothing

            For Each _crv As GH_Curve In _crvList
                If _crv.Value.IsPolyline Then
                    Dim _poly As PolylineCurve = _crv.Value
                    Dim _thisbox As BoundingBox = _poly.GetBoundingBox(_frm)
                    _BBox.Union(_thisbox)
                End If
            Next

            Dim _myBox As New Box(_BBox)
            _X.Add(_myBox.X.Length)
            _Y.Add(_myBox.Y.Length)
        Next

        '2. Sort box dimensions
        Dim _dblList As New List(Of Double)
        _X.Sort()
        _X.Reverse()
        _dblList.Add(_X.Item(0)) '+ 25

        _Y.Sort()
        _Y.Reverse()
        _dblList.Add(_Y.Item(0)) '+ 25

        Return _dblList

    End Function

    Private Function createRectangluarGrid(ByVal _Origin As Point3d, ByVal _Frames As GH_Structure(Of GH_Plane), ByVal _Width As Double, ByVal _Height As Double) As GH_Structure(Of GH_Rectangle)

        Dim _O As Point3d = _Origin
        Dim _F As GH_Structure(Of GH_Plane) = _Frames
        Dim _w As Double = _Width
        Dim _h As Double = _Height

        Dim _rectTree As New GH_Structure(Of GH_Rectangle)
        'Dim _ptTree As New List(Of Point3d)
        Dim _plane As New Plane(_O, Rhino.Geometry.Vector3d.ZAxis)
        Dim _rect As New Rhino.Geometry.Rectangle3d(_plane, _w, _h)

        Dim _numRows As Int32 = Math.Ceiling(Math.Sqrt(_F.Branches.Count))
        Dim _numCols As Int32 = _numRows
        Dim _count As Int32 = 0

        For i As Int32 = 0 To _numRows - 1
            For j As Int32 = 0 To _numCols - 1
                If _count < _F.Branches.Count Then
                    Dim _path As New GH_Path(_count)
                    Dim _cRect As Rectangle3d = _rect
                    Dim _cPt As Point3d = _O
                    Dim check As Boolean = _cRect.Transform(Rhino.Geometry.Transform.Translation(i * _Width, j * _Height, 0))
                    If check Then
                        Dim _ghRect As New GH_Rectangle(_cRect)
                        _rectTree.Append(_ghRect, _path)
                    End If
                    _count += 1
                End If
            Next
        Next

        Return _rectTree
    End Function

    Private Function createTargetPlanes(ByVal _grid As GH_Structure(Of GH_Rectangle)) As GH_Structure(Of GH_Plane)

        Dim _G As GH_Structure(Of GH_Rectangle) = _grid
        Dim _plTree As New GH_Structure(Of GH_Plane)

        For i As Int32 = 0 To _G.Branches.Count - 1

            Dim _path As New GH_Path(i)
            Dim _ghRectangle As GH_Rectangle = _G.Branch(i).Item(0)
            Dim _target As New Plane(_ghRectangle.Value.Center, Vector3d.ZAxis)
            Dim _ghTarget As New GH_Plane(_target)
            _plTree.Append(_ghTarget, _path)
        Next

        Return _plTree
    End Function

    Private Function createGridPoints(ByVal _grid As GH_Structure(Of GH_Rectangle)) As GH_Structure(Of GH_Point)

        Dim _G As GH_Structure(Of GH_Rectangle) = _grid
        Dim _ptTree As New GH_Structure(Of GH_Point)

        For i As Int32 = 0 To _G.Branches.Count - 1

            Dim _path As New GH_Path(i)
            Dim _ghRectangle As Rectangle3d = _G.Branch(i).Item(0).Value
            _ghRectangle.Corner(0)
            Dim _point As Point3d = _ghRectangle.Corner(0)
            Dim _ghPoint As New GH_Point(_point)
            _ptTree.Append(_ghPoint, _path)
        Next

        Return _ptTree
    End Function

    Private Function reorientCurves(ByVal _Curves As GH_Structure(Of GH_Curve), ByVal _Frames As GH_Structure(Of GH_Plane), ByVal _Targets As GH_Structure(Of GH_Plane)) As Grasshopper.DataTree(Of PolylineCurve)

        Dim _C As GH_Structure(Of GH_Curve) = _Curves
        Dim _F As GH_Structure(Of GH_Plane) = _Frames
        Dim _T As GH_Structure(Of GH_Plane) = _Targets

        Dim _polyTree As New Grasshopper.DataTree(Of PolylineCurve)

        For i As Int32 = 0 To _C.Branches.Count - 1

            Dim _path As New GH_Path(i)
            Dim _crvList As List(Of GH_Curve) = _C.Branch(i)
            Dim _target As Plane = _T.Branch(i).Item(0).Value
            Dim _source As Plane = _F.Branch(i).Item(0).Value

            For Each _crv As GH_Curve In _crvList
                If _crv.Value.IsPolyline Then
                    Dim _poly As PolylineCurve = _crv.Value.DuplicateCurve
                    Dim _reorient As New Transforms.Orientation(_source, _target)
                    _poly.Transform(_reorient.ToMatrix)
                    'Dim _ghPoly As New GH_Curve(_poly)
                    _polyTree.Add(_poly, _path)
                End If
            Next
        Next
        Return _polyTree
    End Function
End Class