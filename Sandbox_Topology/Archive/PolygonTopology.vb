Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types




Public Class PolygonTopology
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PolygonTopology class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Topology", "Topology", _
     "Analyses the topology of a curve network consisting of closed polylines", _
     "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddCurveParameter("Polygon network", "C", "Polygon network", GH_ParamAccess.list)
        pManager.AddNumberParameter("Tolerance", "T", "Tolerance value", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddGenericParameter("Collection of point IDs and values", "P", "Point collection", GH_ParamAccess.item)
        pManager.AddGenericParameter("Collection of faces and associated point IDs", "FP", "Face-point collection", GH_ParamAccess.item)
        pManager.AddGenericParameter("Collection of point IDs and associated faces", "PF", "Point-face collection", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)
        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _C As New List(Of GH_Curve)
        Dim _T As Double = 0

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _C)) Then Return
        If (Not DA.GetData(1, _T)) Then Return

        '3. Abort on invalid inputs.
        If (Not _C.Count > 0) Then Return
        If (Not _T > 0) Then Return

        '4. Do something useful.
        Dim _polyList As New List(Of Polyline)

        '4.1. check inputs
        For Each _crv As GH_Curve In _C
            Dim _poly As Polyline = Nothing
            If Not _crv.Value.TryGetPolyline(_poly) Then Return
            _polyList.Add(_poly)
        Next

        '4.2. get topology
        Dim _ptDict As Dictionary(Of String, Point3d) = getPointDict(_polyList)
        Dim _fDict As Dictionary(Of String, List(Of String)) = getFaceDict(_polyList, _ptDict)
        Dim _ptFaceDict As Dictionary(Of String, List(Of String)) = getPointFaceDict(_fDict, _ptDict)
        ''Dim _ptFaceList As List(Of String) = getPointFaceDict(_fDict, _ptDict)

        ' 4.X: return results
        Dim _ptWrapper As New GH_ObjectWrapper(_ptDict)
        Dim _fWrapper As New GH_ObjectWrapper(_fDict)
        Dim _ptFaceWrapper As New Grasshopper.Kernel.Types.GH_ObjectWrapper(_ptFaceDict)

        DA.SetData(0, _ptWrapper)
        DA.SetData(1, _fWrapper)
        DA.SetData(2, _ptFaceWrapper)


    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.Sandbox_Topology
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{df9c3597-cbc2-4a05-b6e5-22eac1049469}")
        End Get
    End Property

    Private Function getPointFaceDict(ByVal _fDict As Dictionary(Of String, List(Of String)), ByVal _ptDict As Dictionary(Of String, Point3d)) As Dictionary(Of String, List(Of String))

        Dim _ptFaceDict As New Dictionary(Of String, List(Of String))

        For Each _ptID As String In _ptDict.Keys

            Dim _fList As New list(Of String)

            'For i As Int32 = 0 To _fDict.Values.Count - 1
            'For Each _values As List(Of String) In _fDict.Values
            For Each _key As String In _fDict.Keys
                Dim _values As List(Of String) = _fDict.Item(_key)

                For j As Int32 = 0 To _values.Count - 1
                    If _ptID = _values.Item(j) Then
                        _fList.Add(_key)
                        '_string = _string & (_fDict.Keys(i)) & ", "
                    End If
                Next

            Next

            _ptFaceDict.Add(_ptID, _fList)

        Next

        Return _ptFaceDict

    End Function

    Private Function getFaceDict(ByVal _polyList As List(Of Polyline), ByVal _ptDict As Dictionary(Of String, Point3d)) As Dictionary(Of String, List(Of String))

        Dim _fDict As New Dictionary(Of String, List(Of String))

        Dim _count As Int32 = 0
        For Each _poly As Polyline In _polyList

            Dim _key As String = "F" & _count

            Dim _points As Point3d()
            _points = _poly.ToArray

            Dim _value As New List(Of String)

            For i As Int32 = 0 To UBound(_points) - 1

                For Each _item As String In _ptDict.Keys
                    If _ptDict.Item(_item) = _points(i) Then
                        _value.Add(_item)
                        Exit For
                    End If
                Next

            Next

            _fDict.Add(_key, _value)

            _count = _count + 1

        Next

        Return _fDict

    End Function

    Private Function getPointDict(ByVal _polyList As List(Of Polyline)) As Dictionary(Of String, Point3d)

        Dim _ptDict As New Dictionary(Of String, Point3d)

        Dim _count As int32 = 0
        For Each _poly As Polyline In _polyList

            Dim _points As Point3d()
            _points = _poly.ToArray

            For i As int32 = 0 To Ubound(_points)

                ' check if point exists in _ptDict already
                If Not _ptDict.ContainsValue(_points(i)) Then
                    Dim _key As String = "P" & _count
                    Dim _value As Point3d = _points(i)
                    _ptDict.Add(_key, _value)
                    _count = _count + 1
                End If

            Next

        Next

        Return _ptDict

    End Function
End Class