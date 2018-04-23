Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Linq
Imports System.Text
Imports DevExpress.DataAccess.Sql
Imports DevExpress.Mvvm.DataAnnotations
Imports DevExpress.Mvvm.POCO

Namespace CustomDataSourceWizard
    Public Class QueryWrapper
        Implements IEditableObject, IDataErrorInfo

        Private Property Cache() As QueryWrapper
        Private ReadOnly Property CurrentModel() As Object
            Get
                Return Me
            End Get
        End Property
        Public Overridable Property Name() As String
        Public Overridable Property Sql() As String
        Friend Property OwnerDataSource() As DataSourceWrapper
        Public Shared Function Create() As QueryWrapper
            Return ViewModelSource.Create(Function() New QueryWrapper())
        End Function
        Protected Sub New()
        End Sub
        Public Function CreateCustomSqlQuery() As CustomSqlQuery
            Return New CustomSqlQuery(Me.Name, Me.Sql)
        End Function
        Public Sub DoCancelEdit()
            Me.CancelEdit()
        End Sub
        Public Function CanCancelEdit() As Boolean
            Return True
        End Function
        Public Sub DoEndEdit()
            Me.EndEdit()
        End Sub
        Public Overridable Function CanEndEdit() As Boolean
            Return True
        End Function
#Region "IEditableObject Members"
        <Command(False)>
        Public Sub BeginEdit() Implements IEditableObject.BeginEdit
            Cache = ViewModelSource.Create(Function() New QueryWrapper())
            For Each info In CurrentModel.GetType().GetProperties()
                If (Not info.CanRead) OrElse (Not info.CanWrite) Then
                    Continue For
                End If
                Dim oldValue = info.GetValue(CurrentModel, Nothing)
                Cache.GetType().GetProperty(info.Name).SetValue(Cache, oldValue, Nothing)
            Next info
        End Sub
        <Command(False)>
        Public Sub CancelEdit() Implements IEditableObject.CancelEdit
            For Each info In CurrentModel.GetType().GetProperties()
                If (Not info.CanRead) OrElse (Not info.CanWrite) Then
                    Continue For
                End If
                Dim oldValue = info.GetValue(Cache, Nothing)
                CurrentModel.GetType().GetProperty(info.Name).SetValue(CurrentModel, oldValue, Nothing)
            Next info
        End Sub
        <Command(False)>
        Public Sub EndEdit() Implements IEditableObject.EndEdit
            Cache = Nothing
        End Sub
#End Region

#Region "IDataErrorInfo Members"
        Public ReadOnly Property [Error]() As String Implements IDataErrorInfo.Error
            Get
                Return Me("Name") & Me("Sql")
            End Get
        End Property
        Default Public ReadOnly Property Item(ByVal columnName As String) As String Implements IDataErrorInfo.Item
            Get
                Dim errorMessage As String = String.Empty
                Select Case columnName
                    Case "Name"
                        If String.IsNullOrEmpty(Me.Name) Then
                            errorMessage = "You cannot leave the Query Name empty."
                        End If
                    Case "Sql"
                        errorMessage = ValidateSql()
                End Select
                Return errorMessage
            End Get
        End Property

        Private Function ValidateSql() As String
            If String.IsNullOrEmpty(Me.Sql) Then
                Return "You cannot leave the Sql empty"
            End If
            Dim sql_Renamed As String = Me.Sql
            Dim testDS As New SqlDataSource()
            testDS.ConnectionName = Me.OwnerDataSource.Connection.ConnectionName
            testDS.ConnectionParameters = Me.OwnerDataSource.Connection.ConnectionParameters
            Try
                testDS.Queries.Add(New CustomSqlQuery(Me.Name, sql_Renamed))
                testDS.RebuildResultSchema()
            Catch e As Exception
                Return e.Message
            End Try
            Return String.Empty
        End Function
#End Region
    End Class
End Namespace