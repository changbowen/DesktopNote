﻿#ExternalChecksum("..\..\MainWindow.xaml","{406ea660-64cf-4c82-b6f0-42d48172a799}","2CBDF061D0CA64FCB96285F81B640272")
'------------------------------------------------------------------------------
' <auto-generated>
'     此代码由工具生成。
'     运行时版本:4.0.30319.18408
'
'     对此文件的更改可能会导致不正确的行为，并且如果
'     重新生成代码，这些更改将会丢失。
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports DesktopNote
Imports System
Imports System.Diagnostics
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Ink
Imports System.Windows.Input
Imports System.Windows.Markup
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects
Imports System.Windows.Media.Imaging
Imports System.Windows.Media.Media3D
Imports System.Windows.Media.TextFormatting
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Windows.Shell
Imports Xceed.Wpf.Toolkit
Imports Xceed.Wpf.Toolkit.Chromes
Imports Xceed.Wpf.Toolkit.Core.Converters
Imports Xceed.Wpf.Toolkit.Core.Input
Imports Xceed.Wpf.Toolkit.Core.Media
Imports Xceed.Wpf.Toolkit.Core.Utilities
Imports Xceed.Wpf.Toolkit.Panels
Imports Xceed.Wpf.Toolkit.Primitives
Imports Xceed.Wpf.Toolkit.PropertyGrid
Imports Xceed.Wpf.Toolkit.PropertyGrid.Attributes
Imports Xceed.Wpf.Toolkit.PropertyGrid.Commands
Imports Xceed.Wpf.Toolkit.PropertyGrid.Converters
Imports Xceed.Wpf.Toolkit.PropertyGrid.Editors
Imports Xceed.Wpf.Toolkit.Zoombox


'''<summary>
'''MainWindow
'''</summary>
<Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>  _
Partial Public Class MainWindow
    Inherits System.Windows.Window
    Implements System.Windows.Markup.IComponentConnector
    
    
    #ExternalSource("..\..\MainWindow.xaml",1)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents Win_Main As MainWindow
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",11)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents Rec_BG As System.Windows.Shapes.Rectangle
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",16)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents RTB_Main As System.Windows.Controls.RichTextBox
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",94)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents CP_Font As System.Windows.Controls.ContentPresenter
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",95)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents CP_Back As System.Windows.Controls.ContentPresenter
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",98)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents CB_Font As System.Windows.Controls.ComboBox
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",143)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents CP_Paper As System.Windows.Controls.ContentPresenter
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",145)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents MI_AutoStart As System.Windows.Controls.MenuItem
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",146)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents MI_AutoDock As System.Windows.Controls.MenuItem
    
    #End ExternalSource
    
    
    #ExternalSource("..\..\MainWindow.xaml",163)
    <System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")>  _
    Friend WithEvents TB_Status As System.Windows.Controls.TextBlock
    
    #End ExternalSource
    
    Private _contentLoaded As Boolean
    
    '''<summary>
    '''InitializeComponent
    '''</summary>
    <System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")>  _
    Public Sub InitializeComponent() Implements System.Windows.Markup.IComponentConnector.InitializeComponent
        If _contentLoaded Then
            Return
        End If
        _contentLoaded = true
        Dim resourceLocater As System.Uri = New System.Uri("/DesktopNote;component/mainwindow.xaml", System.UriKind.Relative)
        
        #ExternalSource("..\..\MainWindow.xaml",1)
        System.Windows.Application.LoadComponent(Me, resourceLocater)
        
        #End ExternalSource
    End Sub
    
    <System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0"),  _
     System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never),  _
     System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes"),  _
     System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"),  _
     System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")>  _
    Sub System_Windows_Markup_IComponentConnector_Connect(ByVal connectionId As Integer, ByVal target As Object) Implements System.Windows.Markup.IComponentConnector.Connect
        If (connectionId = 1) Then
            Me.Win_Main = CType(target,MainWindow)
            Return
        End If
        If (connectionId = 2) Then
            Me.Rec_BG = CType(target,System.Windows.Shapes.Rectangle)
            Return
        End If
        If (connectionId = 3) Then
            Me.RTB_Main = CType(target,System.Windows.Controls.RichTextBox)
            Return
        End If
        If (connectionId = 4) Then
            
            #ExternalSource("..\..\MainWindow.xaml",46)
            AddHandler CType(target,Xceed.Wpf.Toolkit.ColorPicker).SelectedColorChanged, New System.Windows.RoutedPropertyChangedEventHandler(Of System.Nullable(Of System.Windows.Media.Color))(AddressOf Me.ColorChange)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 5) Then
            
            #ExternalSource("..\..\MainWindow.xaml",68)
            AddHandler CType(target,System.Windows.Controls.Button).Click, New System.Windows.RoutedEventHandler(AddressOf Me.PasteAsText)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 6) Then
            
            #ExternalSource("..\..\MainWindow.xaml",72)
            AddHandler CType(target,System.Windows.Controls.Button).Click, New System.Windows.RoutedEventHandler(AddressOf Me.Find)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 7) Then
            
            #ExternalSource("..\..\MainWindow.xaml",86)
            AddHandler CType(target,System.Windows.Controls.Button).Click, New System.Windows.RoutedEventHandler(AddressOf Me.ToggleStrike)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 8) Then
            
            #ExternalSource("..\..\MainWindow.xaml",90)
            AddHandler CType(target,System.Windows.Controls.Button).Click, New System.Windows.RoutedEventHandler(AddressOf Me.ToggleHighlight)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 9) Then
            Me.CP_Font = CType(target,System.Windows.Controls.ContentPresenter)
            Return
        End If
        If (connectionId = 10) Then
            Me.CP_Back = CType(target,System.Windows.Controls.ContentPresenter)
            Return
        End If
        If (connectionId = 11) Then
            Me.CB_Font = CType(target,System.Windows.Controls.ComboBox)
            Return
        End If
        If (connectionId = 12) Then
            
            #ExternalSource("..\..\MainWindow.xaml",99)
            AddHandler CType(target,System.Windows.Controls.Button).Click, New System.Windows.RoutedEventHandler(AddressOf Me.DecreaseSize)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 13) Then
            
            #ExternalSource("..\..\MainWindow.xaml",102)
            AddHandler CType(target,System.Windows.Controls.Button).Click, New System.Windows.RoutedEventHandler(AddressOf Me.IncreaseSize)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 14) Then
            Me.CP_Paper = CType(target,System.Windows.Controls.ContentPresenter)
            Return
        End If
        If (connectionId = 15) Then
            Me.MI_AutoStart = CType(target,System.Windows.Controls.MenuItem)
            Return
        End If
        If (connectionId = 16) Then
            Me.MI_AutoDock = CType(target,System.Windows.Controls.MenuItem)
            Return
        End If
        If (connectionId = 17) Then
            
            #ExternalSource("..\..\MainWindow.xaml",147)
            AddHandler CType(target,System.Windows.Controls.MenuItem).Click, New System.Windows.RoutedEventHandler(AddressOf Me.MenuItem_ResetFormats_Click)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 18) Then
            
            #ExternalSource("..\..\MainWindow.xaml",148)
            AddHandler CType(target,System.Windows.Controls.MenuItem).Click, New System.Windows.RoutedEventHandler(AddressOf Me.MenuItem_ResetSet_Click)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 19) Then
            
            #ExternalSource("..\..\MainWindow.xaml",149)
            AddHandler CType(target,System.Windows.Controls.MenuItem).Click, New System.Windows.RoutedEventHandler(AddressOf Me.MenuItem_Help_Click)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 20) Then
            
            #ExternalSource("..\..\MainWindow.xaml",150)
            AddHandler CType(target,System.Windows.Controls.MenuItem).Click, New System.Windows.RoutedEventHandler(AddressOf Me.MenuItem_Exit_Click)
            
            #End ExternalSource
            Return
        End If
        If (connectionId = 21) Then
            Me.TB_Status = CType(target,System.Windows.Controls.TextBlock)
            Return
        End If
        Me._contentLoaded = true
    End Sub
End Class

