﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3.3E-09")]
        public float ZoomRate {
            get {
                return ((float)(this["ZoomRate"]));
            }
            set {
                this["ZoomRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int OnlySaveEveryNthPosition {
            get {
                return ((int)(this["OnlySaveEveryNthPosition"]));
            }
            set {
                this["OnlySaveEveryNthPosition"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int BodyForMeasuring {
            get {
                return ((int)(this["BodyForMeasuring"]));
            }
            set {
                this["BodyForMeasuring"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int NumberOfRevolutionsPerTest {
            get {
                return ((int)(this["NumberOfRevolutionsPerTest"]));
            }
            set {
                this["NumberOfRevolutionsPerTest"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool TestingIsAktive {
            get {
                return ((bool)(this["TestingIsAktive"]));
            }
            set {
                this["TestingIsAktive"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public float TestingToolDeltaTBase {
            get {
                return ((float)(this["TestingToolDeltaTBase"]));
            }
            set {
                this["TestingToolDeltaTBase"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public int TestingToolDeltaTMultiplicator {
            get {
                return ((int)(this["TestingToolDeltaTMultiplicator"]));
            }
            set {
                this["TestingToolDeltaTMultiplicator"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("500")]
        public int StreamNumberOfLastPositions {
            get {
                return ((int)(this["StreamNumberOfLastPositions"]));
            }
            set {
                this["StreamNumberOfLastPositions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int RadiusMultiplicator {
            get {
                return ((int)(this["RadiusMultiplicator"]));
            }
            set {
                this["RadiusMultiplicator"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int RadiusSummand {
            get {
                return ((int)(this["RadiusSummand"]));
            }
            set {
                this["RadiusSummand"] = value;
            }
        }
    }
}
