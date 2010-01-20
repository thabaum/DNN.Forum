'
' DotNetNukeŽ - http://www.dotnetnuke.com
' Copyright (c) 2002-2010
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Imports DotNetNuke.Entities.Profile

Namespace DotNetNuke.Modules.Forum.ACP

	''' <summary>
	''' A control used to manage various avatar settings.
	''' </summary>
	''' <remarks>Should only be available to forum administrators.</remarks>
	Partial Public Class Avatar
		Inherits ForumModuleBase
		Implements Utilities.AjaxLoader.IPageLoad

#Region "Interfaces"

		''' <summary>
		''' This is required to replace If Page.IsPostBack = False because controls are loaded via Ajax. 
		''' </summary>
		''' <remarks></remarks>
		Protected Sub LoadInitialView() Implements Utilities.AjaxLoader.IPageLoad.LoadInitialView
			BindProfileProperties()

			chkEnableUserAvatar.Checked = objConfig.EnableUserAvatar
			EnableUserAvatar(objConfig.EnableUserAvatar)
			chkEnableProfileAvatar.Checked = objConfig.EnableProfileAvatar
			EnableProfileAvatar(objConfig.EnableProfileAvatar)
			ddlProfileAvatarPropertyName.SelectedValue = objConfig.AvatarProfilePropName.ToString()

			txtUserAvatarPath.Text = objConfig.UserAvatarPath
			txtUserAvatarWidth.Text = objConfig.UserAvatarWidth.ToString()
			txtUserAvatarHeight.Text = objConfig.UserAvatarHeight.ToString()
			txtUserAvatarSizeLimit.Text = objConfig.UserAvatarMaxSize.ToString()
			chkEnableUserAvatarPool.Checked = objConfig.EnableUserAvatarPool
			txtUserAvatarPoolPath.Text = objConfig.UserAvatarPoolPath
			chkEnableSystemAvatar.Checked = objConfig.EnableSystemAvatar
			txtSystemAvatarPath.Text = objConfig.SystemAvatarPath
			chkEnableRoleAvatar.Checked = objConfig.EnableRoleAvatar
			txtRoleAvatarPath.Text = objConfig.RoleAvatarPath

			If chkEnableUserAvatarPool.Checked Then
				rowUserAvatarPoolPath.Visible = True
			Else
				rowUserAvatarPoolPath.Visible = False
			End If

			rowSystemAvatarPath.Visible = chkEnableSystemAvatar.Checked

			If chkEnableRoleAvatar.Checked Then
				rowRoleAvatarPath.Visible = True
			Else
				rowRoleAvatarPath.Visible = False
			End If
		End Sub

#End Region

#Region "Event Handlers"

		''' <summary>
		''' Updates the avatar settings in the data store.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		''' <remarks></remarks>
		Protected Sub cmdUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdUpdate.Click
			Try
				Page.Validate()
				If Page.IsValid = False Then Exit Sub

				'[skeel] has the Rolebased avatar setting been changed?
				Dim RefreshMenu As Boolean = False
				If objConfig.EnableRoleAvatar <> chkEnableRoleAvatar.Checked Then
					RefreshMenu = True
				End If

				' Update settings in the database
				Dim ctlModule As New Entities.Modules.ModuleController
				txtSystemAvatarPath.Text = txtSystemAvatarPath.Text.Replace("\", "/")
				txtUserAvatarPath.Text = txtUserAvatarPath.Text.Replace("\", "/")
				txtUserAvatarPoolPath.Text = txtUserAvatarPoolPath.Text.Replace("\", "/")
				txtRoleAvatarPath.Text = txtRoleAvatarPath.Text.Replace("\", "/")
				If txtSystemAvatarPath.Text.EndsWith("/") = False Then txtSystemAvatarPath.Text += "/"
				If txtUserAvatarPath.Text.EndsWith("/") = False Then txtUserAvatarPath.Text += "/"
				If txtUserAvatarPoolPath.Text.EndsWith("/") = False Then txtUserAvatarPoolPath.Text += "/"
				If txtRoleAvatarPath.Text.EndsWith("/") = False Then txtRoleAvatarPath.Text += "/"

				Utilities.ForumUtils.CheckFolder(txtUserAvatarPath.Text)
				Utilities.ForumUtils.CheckFolder(txtSystemAvatarPath.Text)
				Utilities.ForumUtils.CheckFolder(txtUserAvatarPoolPath.Text)
				Utilities.ForumUtils.CheckFolder(txtRoleAvatarPath.Text)

				'Required value for forum integration
				ctlModule.UpdateModuleSetting(ModuleId, "Provider", "DotNetNuke")
				ctlModule.UpdateModuleSetting(ModuleId, "Type", "Forum")

				ctlModule.UpdateModuleSetting(ModuleId, "EnableUserAvatar", chkEnableUserAvatar.Checked.ToString())
				ctlModule.UpdateModuleSetting(ModuleId, "EnableProfileAvatar", chkEnableProfileAvatar.Checked.ToString())
				ctlModule.UpdateModuleSetting(ModuleId, "AvatarProfilePropName", ddlProfileAvatarPropertyName.SelectedValue)

				ctlModule.UpdateModuleSetting(ModuleId, "EnableUserAvatarPool", chkEnableUserAvatarPool.Checked.ToString())
				ctlModule.UpdateModuleSetting(ModuleId, "UserAvatarPath", txtUserAvatarPath.Text)
				ctlModule.UpdateModuleSetting(ModuleId, "UserAvatarPoolPath", txtUserAvatarPoolPath.Text)
				ctlModule.UpdateModuleSetting(ModuleId, "UserAvatarWidth", txtUserAvatarWidth.Text)
				ctlModule.UpdateModuleSetting(ModuleId, "UserAvatarHeight", txtUserAvatarHeight.Text)
				ctlModule.UpdateModuleSetting(ModuleId, "UserAvatarMaxSize", txtUserAvatarSizeLimit.Text)
				ctlModule.UpdateModuleSetting(ModuleId, "EnableSystemAvatar", chkEnableSystemAvatar.Checked.ToString())
				ctlModule.UpdateModuleSetting(ModuleId, "SystemAvatarPath", txtSystemAvatarPath.Text)
				ctlModule.UpdateModuleSetting(ModuleId, "EnableRoleAvatar", chkEnableRoleAvatar.Checked.ToString())
				ctlModule.UpdateModuleSetting(ModuleId, "RoleAvatarPath", txtRoleAvatarPath.Text)

				Config.ResetForumConfig(ModuleId)

				If RefreshMenu = True Then
					'Force reload of the menu as EnableRoleAvatar has been changed
					Response.Redirect(Utilities.Links.ACPControlLink(TabId, ModuleId, AdminAjaxControl.Avatar))
				Else
					lblUpdateDone.Visible = True
				End If

			Catch exc As Exception
				ProcessModuleLoadException(Me, exc)
			End Try
		End Sub

		''' <summary>
		''' Enables/Disables anon download checkbox depending on if
		''' attachments are permitted or not.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		''' <remarks>Changes viewable/editable items when checked/unchecked.
		''' </remarks>
		Protected Sub chkEnableUserAvatar_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnableUserAvatar.CheckedChanged
			EnableUserAvatar(chkEnableUserAvatar.Checked)
		End Sub

		''' <summary>
		''' Enables/Disables anon download checkbox depending on if
		''' attachments are permitted or not.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		''' <remarks>Changes viewable/editable items when checked/unchecked.
		''' </remarks>
		Protected Sub chkEnableSystemAvatar_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnableSystemAvatar.CheckedChanged
			rowSystemAvatarPath.Visible = chkEnableSystemAvatar.Checked
		End Sub

		''' <summary>
		''' Enables/Disables anon download checkbox depending on if
		''' attachments are permitted or not.
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		''' <remarks>Changes viewable/editable items when checked/unchecked.
		''' </remarks>
		Protected Sub chkEnableUserAvatarPool_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkEnableUserAvatarPool.CheckedChanged
			If chkEnableUserAvatarPool.Checked Then
				rowUserAvatarPoolPath.Visible = True
			Else
				rowUserAvatarPoolPath.Visible = False
			End If
		End Sub

		''' <summary>
		''' 
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		''' <remarks></remarks>
		Protected Sub chkEnableRoleAvatar_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkEnableRoleAvatar.CheckedChanged
			If chkEnableRoleAvatar.Checked Then
				rowRoleAvatarPath.Visible = True
			Else
				rowRoleAvatarPath.Visible = False
			End If
		End Sub

		''' <summary>
		''' 
		''' </summary>
		''' <param name="sender"></param>
		''' <param name="e"></param>
		''' <remarks></remarks>
		Protected Sub chkEnableProfileAvatar_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkEnableProfileAvatar.CheckedChanged
			EnableProfileAvatar(chkEnableProfileAvatar.Checked)
		End Sub

#End Region

#Region "Private Methods"

		''' <summary>
		''' Sets the control up for scenarios where profile avatars are enabled. 
		''' </summary>
		''' <param name="Enabled"></param>
		''' <remarks></remarks>
		Private Sub EnableProfileAvatar(ByVal Enabled As Boolean)
			rowProfileAvatarPropertyName.Visible = Enabled
			rowUserAvatarPath.Visible = Enabled
			rowUserAvatarSizeLimit.Visible = Not Enabled

			If Enabled Then
				rowUserAvatarPoolPath.Visible = False
				chkEnableUserAvatarPool.Checked = False
				rowUserAvatarPoolEnable.Visible = False
			Else
				rowUserAvatarPoolEnable.Visible = True
			End If
		End Sub

		''' <summary>
		''' Binds a list of profile properties avaialble to the portal. 
		''' </summary>
		''' <remarks></remarks>
		Private Sub BindProfileProperties()
			Dim colProfileProps As New ProfilePropertyDefinitionCollection
			colProfileProps = ProfileController.GetPropertyDefinitionsByPortal(PortalId)

			ddlProfileAvatarPropertyName.ClearSelection()
			ddlProfileAvatarPropertyName.DataSource = colProfileProps
			ddlProfileAvatarPropertyName.DataBind()
		End Sub

		''' <summary>
		''' 
		''' </summary>
		''' <param name="Enabled"></param>
		''' <remarks></remarks>
		Private Sub EnableUserAvatar(ByVal Enabled As Boolean)
			rowUserAvatarDimentions.Visible = Enabled
			rowUserAvatarPath.Visible = Enabled
			rowUserAvatarSizeLimit.Visible = Enabled
			rowEnableProfileAvatar.Visible = Enabled
			rowProfileAvatarPropertyName.Visible = False
		End Sub

#End Region

	End Class

End Namespace