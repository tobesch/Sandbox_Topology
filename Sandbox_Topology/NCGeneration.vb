Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class NCGeneration
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the MyComponent1 class.
    ''' </summary>
    Public Sub New()
        MyBase.New("NC Generation", "NC", _
           "Generates NC output from point, vector and feed data", _
           "Sandbox", "CNC")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Points", "P", "NC-Code target points", GH_ParamAccess.tree)
        pManager.AddVectorParameter("Vectors", "V", "NC-Code tool vectors", GH_ParamAccess.tree)
        pManager.AddTextParameter("Feed", "F", "NC-Code feed information", GH_ParamAccess.tree)
        pManager.AddVectorParameter("Orientation", "O", "Orientation vector for the ZRotation", GH_ParamAccess.tree)
        pManager.AddGenericParameter("Settings", "S", "Tool and fabrication settings", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Optimize Z-rotation", "ZROT", "multi-axis strategies. 0: no strategy, just plain old G-Code, 1: Optimize rotation around tool Z-axis (6-axis)", GH_ParamAccess.item, 0.0)
        'pManager.AddIntegerParameter("Optimize Z-rotation", "ZROT", "multi-axis strategies. 0: no strategy, just plain old G-Code, 1: Optimize rotation around tool Z-axis (6-axis), 2: Optimize Rotation of external axis (7-axis)", GH_ParamAccess.item, 0.0)
        'pManager.AddNumberParameter("EXTAX-rotation value", "EXT", "sets the rotation angle for the external axis, only applicable if ZROT strategy is 2", GH_ParamAccess.item, 0.0)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("NC-Code", "NC", "NC-Code output", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim P As GH_Structure(Of GH_Point) = Nothing
        Dim V As GH_Structure(Of GH_Vector) = Nothing
        Dim F As GH_Structure(Of GH_String) = Nothing
        Dim _O As GH_Structure(Of GH_Vector) = Nothing
        Dim _W As GH_ObjectWrapper = Nothing
        'Dim S As New List(Of NCSettingType)
        Dim zrot As Int32 = 0
        Dim _ext As Double = 0.0

        '2. Retrieve input data.
        If (Not DA.GetDataTree(0, P)) Then Return
        If (Not DA.GetDataTree(1, V)) Then Return
        If (Not DA.GetDataTree(2, F)) Then Return
        If (Not DA.GetDataTree(3, _O)) Then Return
        If (Not DA.GetData(4, _W)) Then Return
        If (Not DA.GetData(5, zrot)) Then Return
        'If (Not DA.GetData(5, _ext)) Then Return

        Dim S As List(Of String) = _W.Value

        'Dim _arr As String() = S.Item(7).Split(",")
        'Dim _x As Double = CDbl(_arr(0).Trim("{"))
        'Dim _y As Double = CDbl(_arr(1))
        'Dim _z As Double = CDbl(_arr(2).Trim("}"))
        'Dim _RPOS As New Vector3d(_x, _y, _z)
        '_RPOS.Reverse()

        'AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, _RPOS.ToString)

        '3. Abort on invalid inputs.
        '3.1. get the number of branches in the trees
        Dim iPathsP As Int32 = P.Branches.Count
        Dim iPathsV As Int32 = V.Branches.Count
        Dim iPathsF As Int32 = F.Branches.Count
        '3.2. compare numer of branches in the trees
        If iPathsP <> iPathsF Or iPathsP <> iPathsV Or iPathsF <> iPathsV Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The tree sizes of P, F and V don't match!")
            Return
        End If

        If S.Count = 0 Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tool settings are undefined!")
            Return
        End If

        '4. Do something useful.
        Dim GSatz As New List(Of String)

        'compose the header
        For k As Int32 = 0 To 5
            GSatz.Add("N" & CStr(GSatz.Count + 1) & " " & S.Item(k))
        Next

        '4.1 check if zrot is true
        Dim _zrotInstruction As List(Of String) = Nothing
        If CInt(zrot) = 0 Then
            ' do nothing
        ElseIf CInt(zrot) = 1 Then
            ' do z rotation optimization
            _zrotInstruction = doZROToptimization(P, V, _O)
        ElseIf CInt(zrot) = 2 Then
            ' do external axis rotation optimization
            _zrotInstruction = doEXAXoptimization(P, V, _ext)
        ElseIf CInt(zrot) = 3 Then
            ' do external axis rotation optimization
            _zrotInstruction = doEXAXoptimization_2(P, V, _ext)
            GSatz.Add(vbCrLf)
            GSatz.Add("TCPZROT / -45")
        Else
            zrot = 0
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No optimization strategy is indexed with this value.")
        End If

        '4.2 start composing the g-code
        Dim G As String = String.Empty
        Dim R As Int32 = 3
        Dim count As Int32 = 7

        For i As Int32 = 0 To iPathsP - 1

            Dim ptList As List(Of GH_Point) = P.Branch(i)
            Dim vtList As List(Of GH_Vector) = V.Branch(i)
            Dim fdList As List(Of GH_String) = F.Branch(i)

            GSatz.Add(vbCrLf)

            If CInt(zrot) <> 0 Then GSatz.Add(_zrotInstruction.Item(i))

            For j As Int32 = 0 To ptList.Count - 1

                Dim oPt As Point3d = ptList.Item(j).Value
                Dim oVt As Vector3d = vtList.Item(j).Value
                Dim oFd As String = fdList.Item(j).Value

                G = "X" & CStr(Math.Round(oPt.X, R)) & " Y" & CStr(Math.Round(oPt.Y, R)) & " Z" & CStr(Math.Round(oPt.Z, R)) _
                  & " I" & CStr(Math.Round(oVt.X, R)) & " J" & CStr(Math.Round(oVt.Y, R)) & " K" & CStr(Math.Round(oVt.Z, R))

                If j = 0 Then
                    'GSatz.Add(vbCrlf & "TCPZROT / " & CStr(Math.Round(zRotList.Item(0), R)))
                    GSatz.Add("N" & (count) & " " & oFd & " " & G)
                    If CInt(zrot) = 2 Then GSatz.Add("EXTAXISTURN / ON ZTOOL 50")
                    'ElseIf F.Branch(i).Item(j).Value = "G0" And j <> 0 Then
                    'GSatz.Add("N" & (count) & " " & oFd & " " & G)
                ElseIf j = 1 Then
                    GSatz.Add("N" & (count) & " " & oFd & " " & G & " " & "F" & CStr(S.Item(6)))
                ElseIf (j = ptList.Count - 2) And (CInt(zrot) = 2) Then
                    GSatz.Add("N" & (count) & " " & oFd & " " & G)
                    GSatz.Add("EXTAXISTURN / OFF")
                Else
                    GSatz.Add("N" & (count) & " " & oFd & " " & G)
                End If

                count += 1

            Next

        Next
        GSatz.Add(vbCrLf)
        'GSatz.Add("EXTAXISTURN / 0")
        'GSatz.Add("N" & CStr(count) & " G0 X-337.29 Y734.55 Z701.48 I0.0 J-0.0 K1.0")
        GSatz.Add(vbCrLf & "N" & CStr(count + 1) & " M30")

        DA.SetDataList(0, GSatz)
    End Sub

    Private Function doEXAXoptimization_2(ByVal P As GH_Structure(Of GH_Point), ByVal V As GH_Structure(Of GH_Vector), ByVal _ext As Double) As List(Of String)

        Dim _zrotList As New List(Of String)

        For i As Int32 = 0 To P.Branches.Count - 1

            '1.1  Find centroid of point list
            Dim _centroid As Point3d = findCentroidXY(P.Branch(i))
            Dim _avgVt As Vector3d = findAvgVector(V.Branch(i))
            Dim _avgVtXY As New Vector3d(_avgVt.X, _avgVt.Y, 0)

            '1.2  compute angle between X-axis and toolpath centroid)
            Dim _alpha As Double = Rhino.Geometry.Vector3d.VectorAngle(Vector3d.XAxis, New Vector3d(_centroid), Plane.WorldXY)
            If _alpha > Math.PI Then
                _alpha = _alpha - 2 * Math.PI
            End If

            '1.3 constant orientation of the EXAX
            Dim _result As Double = _ext - Rhino.RhinoMath.ToDegrees(_alpha)

            'If _result > 180 Then
            '    _result = _result - 360
            'End If

            _zrotList.Add("EXTAXISTURN / " & CStr(Math.Round(_result, 3)))

        Next
        Return _zrotList

    End Function

    Private Function doEXAXoptimization(ByVal P As GH_Structure(Of GH_Point), ByVal V As GH_Structure(Of GH_Vector), ByVal _ext As Double) As List(Of String)

        Dim _zrotList As New List(Of String)

        For i As Int32 = 0 To P.Branches.Count - 1

            '1.1  Find centroid of point list
            Dim _centroid As Point3d = findCentroidXY(P.Branch(i))
            Dim _avgVt As Vector3d = findAvgVector(V.Branch(i))
            Dim _avgVtXY As New Vector3d(_avgVt.X, _avgVt.Y, 0)

            '1.2  compute angle between X-axis and average tool vector _avgVtXY)
            '     Roboterorientierung in Bezug zur X-Achse
            Dim _alpha As Double = Rhino.Geometry.Vector3d.VectorAngle(Vector3d.XAxis, _avgVtXY, Plane.WorldXY)
            If _alpha > Math.PI Then
                _alpha = _alpha - 2 * Math.PI
            End If

            '1.3 constant orientation of the EXAX
            Dim _result As Double = _ext - Rhino.RhinoMath.ToDegrees(_alpha)
            _zrotList.Add("EXTAXISTURN / " & CStr(Math.Round(_result, 3)))

        Next
        Return _zrotList

    End Function

    Private Function doZROToptimization(ByVal P As GH_Structure(Of GH_Point), ByVal V As GH_Structure(Of GH_Vector), ByVal O As GH_Structure(Of GH_Vector)) As List(Of String)

        Dim _zrotList As New List(Of String)

        For i As Int32 = 0 To P.Branches.Count - 1

            '1.1  Find centroid of point list
            Dim _ori As GH_Vector = Nothing
            If O.Branch(i).Count = 0 Then
                _ori = O.Branch(i).Item(0)
            Else
                _ori = O.Branch(i).Item(0)
            End If

            Dim _avgVt As Vector3d = findAvgVector(V.Branch(i))
            Dim _avgVtXY As New Vector3d(_avgVt.X, _avgVt.Y, 0)

            '1.2  compute _alpha (horizontal angle between X-axis and Robot Axis 0)
            '     Roboterorientierung in Bezug zur X-Achse
            'Dim _dir As Vector3d = Vector3d.Subtract(_centroid, RPOS)
            Dim _alpha As Double = Vector3d.VectorAngle(Vector3d.XAxis, _ori.Value, Plane.WorldXY)
            If _alpha > Math.PI Then
                _alpha = _alpha - 2 * Math.PI
            End If

            'AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, Rhino.RhinoMath.ToDegrees(_alpha))

            '1.3  compute _beta (angle between _avgVt and _dir)
            '     Toolwinkel in Bezug auf Roboterorientierung
            Dim _beta As Double = Rhino.Geometry.Vector3d.VectorAngle(_ori.Value, _avgVtXY, Plane.WorldXY)
            If _beta > Math.PI Then
                _beta = _beta - 2 * Math.PI
            End If

            '1.4  compute _beta (angle between X-axis and _avgVt)
            '     Toolwinkel in Bezug zur X-Achse
            Dim _beta2 As Double = Rhino.Geometry.Vector3d.VectorAngle(Vector3d.XAxis, _avgVtXY, Plane.WorldXY)
            If _beta2 > Math.PI Then
                _beta2 = _beta2 - 2 * Math.PI
            End If

            '1.5  compute _gamma (angle between Z-axis and _avgVt)
            '     Toolwinkel in Bezug zur Z-Achse
            Dim _gamma As Double = Rhino.Geometry.Vector3d.VectorAngle(Vector3d.ZAxis, _avgVt)

            '1.6 compute ZROT value
            Dim _zrot As Double = Rhino.RhinoMath.ToDegrees(_alpha) ', _beta2, _gamma)
            'Dim _zrot As Double = defineZROT(_alpha) ', _beta2, _gamma)

            _zrotList.Add("TCPZROT / " & CStr(Math.Round(_zrot, 3)))


        Next

        Return _zrotList

    End Function

    Private Function defineZROT(ByVal _a As Double) As Double
        'Private Function defineZROT(ByVal _a As Double, ByVal _b As Double, ByVal _z As Double) As Double

        Dim alpha As Double = Rhino.RhinoMath.ToDegrees(_a)
        'Dim beta As Double = Rhino.RhinoMath.ToDegrees(_b)
        'Dim zeta As Double = Rhino.RhinoMath.ToDegrees(_z)

        Dim _zrot As Double = 0

        'If zeta < 2 Then
        '_zrot = alpha
        'Return _zrot
        'End If

        If alpha > 0 Then
            'If Math.Abs(beta) <= 45 Then
            '    _zrot = beta
            'ElseIf beta > 45 Then
            '    _zrot = beta - 90
            'ElseIf beta < -45 And beta >= -135 Then
            '    _zrot = beta + 90
            'ElseIf beta < -135 Then
            '    _zrot = beta + 270
            'End If
            _zrot = Math.Abs(alpha - 45) + 45
        Else
            'If System.Math.Abs(beta) <= 45 Then
            '    _zrot = beta
            'ElseIf beta < -45 Then
            '    _zrot = beta + 90
            'ElseIf beta > 45 And beta <= 135 Then
            '    _zrot = beta - 90
            'ElseIf beta > 135 Then
            '    _zrot = beta - 270
            'End If
            _zrot = -Math.Abs(alpha + 45) - 45
        End If

        Return _zrot

        'If System.Math.Abs(beta) < 90 Then
        '  A = beta
        'Else
        '  If alpha > 0 And beta > 0 Then
        '    A = beta - 90
        '  ElseIf alpha > 0 And beta < 0 Then
        '   A = beta + 90
        ' ElseIf alpha < 0 And beta < 0 Then
        '    A = beta + 90
        '  ElseIf alpha < 0 And beta > 0 Then
        '    If beta < 135 Then
        '      A = beta - 90
        '    Else
        '      A = beta - 180 - 90
        '    End If
        '  End If
        'End If
    End Function

    Private Function findAvgVector(ByVal V As List(Of GH_Vector)) As Vector3d
        Dim _avg As New Vector3d(0, 0, 0)
        For Each _vec As GH_Vector In V
            _avg = Vector3d.Add(_avg, _vec.Value)
        Next
        _avg = _avg / V.Count
        Return _avg
    End Function

    Private Function findCentroidXY(ByVal P As List(Of GH_Point)) As Point3d
        Dim _avg As New Point3d(0, 0, 0)
        For Each _pt As GH_Point In P
            _avg = Point3d.Add(_avg, _pt.Value)
        Next
        _avg.Z = 0
        _avg = _avg / P.Count
        Return _avg
    End Function

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_3_NC
            'Return Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{1ee23cf3-99de-4618-adf4-0cc6f6976481}")
        End Get
    End Property
End Class