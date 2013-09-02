Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports Grasshopper.Kernel.Data


Public Class PolygonPlanarize
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PolygonPlanarize class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Planarize", "Planarize", _
     "Uses gradient decent method to incrementally planarize a polygon network", _
     "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Point collection", "P", "Collection of point IDs and values", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Face-point collection", "FP", "Collection of faces and associated point IDs", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Point-face collection", "PF", "Collection of point IDs and associated faces", GH_ParamAccess.tree)
        pManager.AddPointParameter("Anchor points", "Anc", "List of anchor points", GH_ParamAccess.list, New List(Of Point3d))
        pManager.AddIntegerParameter("Iterations", "Iter", "Number of iterations", GH_ParamAccess.item, 100)
        pManager.AddNumberParameter("Learning rate", "alpha", "Learning rate", GH_ParamAccess.item, 1.0)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddCurveParameter("Planarized polygons", "A", "List of planarized polygons", GH_ParamAccess.list)
        pManager.AddNumberParameter("Cost", "J", "List of (decreasing) cost per iteration", GH_ParamAccess.list)
        'pManager.AddTextParameter("Points", "P", "Blabla", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _P As New List(Of GH_Point)
        Dim _FP As GH_Structure(Of GH_Integer) = Nothing
        Dim _PF As GH_Structure(Of GH_Integer) = Nothing
        Dim _Anc As New List(Of GH_Point)
        Dim _iter As Int32 = Int32.MinValue
        Dim _alpha As Double = Double.NaN

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _P)) Then Return
        If (Not DA.GetDataTree(1, _FP)) Then Return
        If (Not DA.GetDataTree(2, _PF)) Then Return
        If (Not DA.GetDataList(3, _Anc)) Then Return
        If (Not DA.GetData(4, _iter)) Then Return
        If (Not DA.GetData(5, _alpha)) Then Return

        '3. Abort on invalid inputs.
        If (Not _P.Count > 0) Then Return
        If (Not _FP.DataCount > 0) Then Return
        If (Not _PF.DataCount > 0) Then Return
        'If (Not _Anc.Count > 0) Then Return
        If (Not _iter > 0) Then Return
        If (Not _alpha > 0) Then Return

        '4. Do something useful.
        Dim _polyList As New List(Of Polyline)

        ' Step 1: get topology data
        'Dim _ptDict As Dictionary(Of String, Point3d) = _P.Value
        'Dim _fDict As Dictionary(Of String, List(Of String)) = _FP.Value
        'Dim _ptFaceDict As Dictionary(Of String, List(Of String)) = _PF.Value

        ' Step 2: define anchor points
        Dim _ancList As List(Of Int32) = getAnchorPoints(_Anc, _P)

        ' Step 3: gradient descent
        'Dim _planarDict As Dictionary(Of String, Point3d) = gradientDescent(_ptFaceDict, _fDict, _ptDict, _ancList, Iter, alpha)
        Dim _result As gradientResults = gradientDescent(_PF, _FP, _P, _ancList, _iter, _alpha)

        '' Step 4: construct new polylines
        Dim _planarPoly As List(Of Polyline) = getPolylines(_FP, _result.points)

        '' Step 5: output points and metric
        DA.SetDataList(0, _planarPoly)
        DA.SetDataList(1, _result.cost)
        'DA.SetDataList(0, _ancList)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.Sandbox_Planarize
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{17246b1e-83f7-4755-857d-754e305c7cdf}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    Private Structure gradientResults
        Dim points As List(Of GH_Point)
        Dim cost As List(Of Double)
    End Structure

    Private Function gradientDescent(ByVal _PF As GH_Structure(Of GH_Integer), ByVal _FP As GH_Structure(Of GH_Integer), ByVal _P As List(Of GH_Point), ByVal _ancList As List(Of Int32), ByVal _iter As Int32, ByVal _alpha As Double) As gradientResults

        Dim _result As gradientResults
        Dim _cost As New List(Of Double)

        Dim _planarP As New List(Of GH_Point)(_P)

        ' Step 3: gradient descent
        For i As Int32 = 0 To _iter - 1

            Dim _sumDist As Double = 0

            ' Define closest fit plane for each face
            Dim _facePlaneDict As Dictionary(Of Int32, Plane) = getFacePlane(_FP, _planarP)

            ' Loop over every point-faces pair
            For j As Int32 = 0 To _PF.Branches.Count - 1

                Dim _branch As List(Of GH_Integer) = _PF.Branches(j)
                ' get the planes that are associated with each face
                Dim _plList As New List(Of Plane)
                For Each _fID As GH_Integer In _branch
                    _plList.Add(_facePlaneDict.Item(_fID.Value))
                Next

                ' Step 3.1: calculate Cost
                Dim _ptID As Int32 = j

                Dim _point As GH_Point = _planarP.Item(_ptID)

                For Each _plane As Plane In _plList

                    Dim _dist As Double = Math.Abs(_plane.DistanceTo(_point.Value))
                    'Print(_dist)
                    _sumDist = _sumDist + _dist '^ 2 ' square error

                Next

                ' Step 3.2: calculate translation vector
                Dim _translation As New Vector3d(0, 0, 0)
                For Each _plane As Plane In _plList
                    Dim _clPt As Point3d = _plane.ClosestPoint(_point.Value)
                    Dim _clVt As Vector3d = _point.Value - _clPt
                    _translation = _translation + _clVt
                Next
                _translation = _translation / _plList.Count * -_alpha

                ' Step 3.2: update point
                Dim _xform As Transform = Transform.Translation(_translation)

                If Not _ancList.Contains(_ptID) Then
                    _point.Transform(_xform)
                    _planarP.Item(_ptID) = _point
                End If

            Next

            Dim _m As Int32 = _P.Count
            Dim _J As Double = 1 / _m * _sumDist
            _cost.Add(_sumDist)

        Next


        _result.points = _planarP
        _result.cost = _cost

        Return _result

    End Function

    Private Function getFacePlane(ByVal _FP As GH_Structure(Of GH_Integer), ByVal _P As List(Of GH_Point)) As Dictionary(Of Int32, Plane)

        Dim _facePlaneDict As New Dictionary(Of Int32, Plane)
        Dim _points As New List(Of Point3d)

        For Each _branch As List(Of GH_Integer) In _FP.Branches

            _points.Clear()
            For Each _ptID As GH_Integer In _branch
                _points.Add(_P.Item(_ptID.Value).Value)
            Next

            Dim _plane As Plane = Plane.Unset
            If Plane.FitPlaneToPoints(_points.ToArray, _plane) = PlaneFitResult.Success Then
                _facePlaneDict.Add(_facePlaneDict.Keys.Count, _plane)
            Else
                Print("Problem fitting the plane!")
                Return Nothing
            End If

        Next

        Return _facePlaneDict

    End Function


    Private Function containsPoint(ByVal _P As List(Of GH_Point), ByVal _check As Point3d) As Int32

        For i As Int32 = 0 To _P.Count - 1
            'For Each _pt As GH_Point In _P
            Dim _pt As GH_Point = _P.Item(i)
            If _pt.Value.CompareTo(_check) = 0 Then
                'consider it the same point
                Return i
            End If
        Next

        Return -1

    End Function

    Private Function getAnchorPoints(ByVal Anc As List(Of GH_Point), ByVal _P As List(Of GH_Point)) As List(Of Int32)

        Dim _ancList As New List(Of Int32)

        For Each _anc As GH_Point In Anc

            Dim _r As Int32 = containsPoint(_P, _anc.Value)
            If _r > -1 Then _ancList.Add(_r)

        Next

        Return _ancList

    End Function

    Private Function getPolylines(ByVal _FP As GH_Structure(Of GH_Integer), ByVal _P As List(Of GH_Point)) As List(Of Polyline)

        Dim _polyList As New List(Of Polyline)

        For Each _branch As List(Of GH_Integer) In _FP.Branches
            'Print(_ptDict.Keys(1))
            'Dim _ptList As List(Of String) = _pair.Value
            Dim _points(_branch.Count) As Point3d

            For i As Int32 = 0 To UBound(_points) - 1
                'Print(_ptList.item(i))
                _points(i) = _P.Item(_branch.Item(i).Value).Value
            Next
            _points(_branch.Count) = _P.Item(_branch.Item(0).Value).Value

            Dim _poly As New Polyline(_points)

            _polyList.Add(_poly)

        Next

        Return _polyList

    End Function
End Class