Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports System.Drawing


Public Class LabelElements
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the LabelElements class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Label Elements", "Label", _
           "Labels elements for fabrication using a single line font (e.g. Machine Tool Sansserif)", _
           "Sandbox", "Fabrication")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddTextParameter("Labels", "S", "Labels as text", GH_ParamAccess.item)
        pManager.AddTextParameter("Font", "F", "Font used for labelling", GH_ParamAccess.item, "Arial")
        pManager.AddIntegerParameter("Size", "Size", "Size of the font", GH_ParamAccess.item, 10)
        'pManager.AddNumberParameter("Precision", "Prec", "", GH_ParamAccess.item)
        pManager.AddPlaneParameter("Planes", "P", "Planes on which the labels will be placed", GH_ParamAccess.item)

    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddCurveParameter("Labels", "A", "Labels for fabrication", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _S As String = String.Empty
        Dim _F As String = String.Empty
        Dim _size As Int32 = Int32.MinValue
        Dim _prec As Double = 100
        Dim _P As Plane = Plane.Unset

        '2. Retrieve input data.
        If (Not DA.GetData(0, _S)) Then Return
        If (Not DA.GetData(1, _F)) Then Return
        If (Not DA.GetData(2, _size)) Then Return
        If (Not DA.GetData(3, _P)) Then Return

        '3. Abort on invalid inputs.
        If (String.IsNullOrEmpty(_S)) Then Return
        If (String.IsNullOrEmpty(_F)) Then Return
        If (Not _size > 0) Then Return
        If (Not _P.IsValid) Then Return

        '4. Do something useful.
        Dim _polyList As New List(Of Polyline)

        Dim local_font As New Font(_F, _size) 'makes a font instance copy

        Dim _path As New System.Drawing.Drawing2D.GraphicsPath() 'Makes a graphics path object instance
        _path.AddString(_S, local_font.FontFamily, local_font.Style, local_font.Size, New PointF(0, 0), New StringFormat) 'puts the string in

        Dim _matrix As New Drawing2D.Matrix 'This is a transformation matrix.
        _matrix.Reset() 'this turns the transformation matrix into an identity matrix
        _path.Flatten(_matrix, (_size / _prec)) 'this basically turns the path into a polyline that approximates the path

        Dim _pts As PointF() = _path.PathPoints 'extracts the points from the path
        Dim _tps As Byte() = _path.PathTypes 'extracts the PathTypes from the path

        Dim _strokes As New List(Of Polyline) 'empty list of polylines
        Dim _stroke As Polyline = Nothing 'empty polyline

        Dim _typ_start As Byte = Convert.ToByte(Drawing2D.PathPointType.Start) 'I think this figures out which points are start points

        'This some sort of loop using "Do"
        Dim i As Int32 = -1
        Do
            i += 1

            'this is a conditional that is tested
            If (i >= _pts.Length) Then ' if the loop iterator is greater than or equal to the number of PointF() extracted from the path.
                If (_stroke IsNot Nothing) AndAlso (_stroke.Count > 1) Then 'If there's already
                    _strokes.Add(_stroke)
                End If
                Exit Do
            End If

            Select Case _tps(i)
                Case _typ_start
                    If (_stroke IsNot Nothing) AndAlso (_stroke.Count > 1) Then
                        _strokes.Add(_stroke)
                    End If

                    _stroke = New Polyline() 'creates a new stroke if it is a start point
                    _stroke.Add(_pts(i).X, -_pts(i).Y, 0) 'adds the points to the stroke.

                Case Else ' otherwise just add the next point to the stroke

                    _stroke.Add(_pts(i).X, -_pts(i).Y, 0)
            End Select
        Loop

        '##### Added by Tobias Schwinn, 30 March 2013

        'For j As Int32 = 0 To strokes.Count() - 1
        '  strokes(j).Transform(Transform.PlaneToPlane(Plane.WorldXY, plane))
        'Next

        ' center the strokes on the plane
        Dim _cPt As New Point3d(0, 0, 0)
        For j As Int32 = 0 To UBound(_pts)
            _cPt.X = _cPt.X + _pts(j).X
            _cPt.Y = _cPt.Y + _pts(j).Y
        Next
        _cPt.X = _cPt.X / (UBound(_pts) + 1)
        _cPt.Y = -_cPt.Y / (UBound(_pts) + 1)

        Dim _fromPlane As New Plane(_cPt, Vector3d.XAxis, Vector3d.YAxis)
        'Dim _fromPlane As Plane = Plane.WorldXY

        For j As Int32 = 0 To _strokes.Count() - 1
            _strokes(j).Transform(Transform.PlaneToPlane(_fromPlane, _P))
        Next

        ' 5. Add to output parameters
        DA.SetDataList(0, _strokes)


    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.Sandbox_Labels
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{c605ca98-9af1-4801-86c6-dd258dff5fcb}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property
End Class