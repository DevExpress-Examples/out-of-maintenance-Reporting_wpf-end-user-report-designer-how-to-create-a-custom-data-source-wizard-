Imports System.Windows
Imports System.Windows.Input
Imports CustomDataSourceWizard
Imports DevExpress.DataAccess.Sql
Imports DevExpress.Mvvm
Imports DevExpress.Mvvm.UI.Native
Imports DevExpress.XtraReports.UI

Namespace WpfReportDesignerDataSourceWizard
	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Inherits DevExpress.Xpf.Core.DXWindow

		Public Shared RunDataSourceWizardCommandProperty As DependencyProperty

		Shared Sub New()
			DependencyPropertyRegistrator(Of MainWindow).[New]().Register("RunDataSourceWizardCommand", RunDataSourceWizardCommandProperty, DirectCast(Nothing, ICommand))
		End Sub

		Public Property RunDataSourceWizardCommand() As ICommand
			Get
				Return DirectCast(GetValue(RunDataSourceWizardCommandProperty), ICommand)
			End Get
			Set(ByVal value As ICommand)
				SetValue(RunDataSourceWizardCommandProperty, value)
			End Set
		End Property

		Public Sub New()
			RunDataSourceWizardCommand = New DelegateCommand(AddressOf RunDataSourceWizard)
			InitializeComponent()
		End Sub

		Private Sub RunDataSourceWizard()
			Dim wizard = New WizardDialog()
			AddHandler wizard.EndWizard, AddressOf wizard_EndWizard
			wizard.ShowDialog()
		End Sub

		Private Sub wizard_EndWizard(ByVal sender As Object, ByVal e As CustomClosingEventArgs)
			If e.CloseMode = CloseMode.Cancel Then
				Return
			End If
			Dim report As XtraReport = CreateReport(e.DataSource, e.DataMember)
			reportDesigner.NewDocument(report)
		End Sub

		Private Shared Function CreateReport(ByVal ds As SqlDataSource, ByVal dataMember As String) As XtraReport
			Dim report = New XtraReport With {
				.DataSource = ds,
				.DataMember = dataMember
			}
			Return report
		End Function
	End Class
End Namespace
