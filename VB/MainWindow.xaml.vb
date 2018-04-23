Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports CustomDataSourceWizard
Imports DevExpress.DataAccess.Sql
Imports DevExpress.Mvvm
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.Core.Commands
Imports DevExpress.Xpf.Reports.UserDesigner
Imports DevExpress.Xpf.Reports.UserDesigner.Native
Imports DevExpress.XtraReports.UI

Namespace WpfReportDesignerDataSourceWizard
	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Inherits DevExpress.Xpf.Core.DXWindow

		Private Shared reportDesigner As New ReportDesigner()
		Private Shared wizard As WizardDialog
		Public Shared ReadOnly Property RunDataSourceWizardCommand() As DelegateCommand
			Get
				Return New DelegateCommand(AddressOf RunDataSourceWizard)
			End Get
		End Property
		Public Sub New()
			InitializeComponent()
			Me.Content = reportDesigner
		End Sub
		Private Shared Sub RunDataSourceWizard()
			wizard = New WizardDialog()
			AddHandler wizard.EndWizard, AddressOf wizard_EndWizard
			wizard.ShowDialog()
		End Sub
		Private Shared Sub wizard_EndWizard(ByVal sender As Object, ByVal e As CustomClosingEventArgs)
			If e.CloseMode = CloseMode.Cancel Then
				Return
			End If
			Dim report As XtraReport = CreateReport(e.DataSource, e.DataMember)
			reportDesigner.OpenDocument(report)
		End Sub
		Private Shared Function CreateReport(ByVal ds As SqlDataSource, ByVal dataMember As String) As XtraReport
			Dim report As New XtraReport()
			report.DataSource = ds
			report.DataMember = dataMember
			Return report
		End Function
	End Class
End Namespace
