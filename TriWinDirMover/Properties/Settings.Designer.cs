//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TriWinDirMover.Properties
{
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
	internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
	{
		private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

		public static Settings Default
		{
			get
			{
				return defaultInstance;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("True")]
		public bool RunAsAdmin
		{
			get
			{
				return ((bool)(this["RunAsAdmin"]));
			}
			set
			{
				this["RunAsAdmin"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("True")]
		public bool RunPreCommandsAsAdmin
		{
			get
			{
				return ((bool)(this["RunPreCommandsAsAdmin"]));
			}
			set
			{
				this["RunPreCommandsAsAdmin"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("False")]
		public bool AutoResize
		{
			get
			{
				return ((bool)(this["AutoResize"]));
			}
			set
			{
				this["AutoResize"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public global::System.Collections.Specialized.StringCollection DirectorySets
		{
			get
			{
				return ((global::System.Collections.Specialized.StringCollection)(this["DirectorySets"]));
			}
			set
			{
				this["DirectorySets"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public global::System.Collections.Specialized.StringCollection PreCommands
		{
			get
			{
				return ((global::System.Collections.Specialized.StringCollection)(this["PreCommands"]));
			}
			set
			{
				this["PreCommands"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public global::System.Collections.Specialized.StringCollection DisabledItems
		{
			get
			{
				return ((global::System.Collections.Specialized.StringCollection)(this["DisabledItems"]));
			}
			set
			{
				this["DisabledItems"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("True")]
		public bool CalculateSizes
		{
			get
			{
				return ((bool)(this["CalculateSizes"]));
			}
			set
			{
				this["CalculateSizes"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("True")]
		public bool ShowIsDisabled
		{
			get
			{
				return ((bool)(this["ShowIsDisabled"]));
			}
			set
			{
				this["ShowIsDisabled"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("False")]
		public bool KeepCmdOpen
		{
			get
			{
				return ((bool)(this["KeepCmdOpen"]));
			}
			set
			{
				this["KeepCmdOpen"] = value;
			}
		}
	}
}
