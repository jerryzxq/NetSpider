﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18444
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.18444 版自动生成。
// 
#pragma warning disable 1591

namespace Vcredit.WeiXin.RestService.RCWebService {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Data;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="RCWebServiceSoap", Namespace="http://tempuri.org/")]
    public partial class RCWebService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback GetRuleResultByCustomOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetRuleResultByCustomWithIdentityNoOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetRuleResultByCustomWithIdentityNoCallFromOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetRuleResultByCustomAndVersionOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetNetCreditParaOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetMODListAllOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetRCResultOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public RCWebService() {
            this.Url = global::Vcredit.WeiXin.RestService.Properties.Settings.Default.Vcredit_WeiXin_RestService_RCWebService_RCWebService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GetRuleResultByCustomCompletedEventHandler GetRuleResultByCustomCompleted;
        
        /// <remarks/>
        public event GetRuleResultByCustomWithIdentityNoCompletedEventHandler GetRuleResultByCustomWithIdentityNoCompleted;
        
        /// <remarks/>
        public event GetRuleResultByCustomWithIdentityNoCallFromCompletedEventHandler GetRuleResultByCustomWithIdentityNoCallFromCompleted;
        
        /// <remarks/>
        public event GetRuleResultByCustomAndVersionCompletedEventHandler GetRuleResultByCustomAndVersionCompleted;
        
        /// <remarks/>
        public event GetNetCreditParaCompletedEventHandler GetNetCreditParaCompleted;
        
        /// <remarks/>
        public event GetMODListAllCompletedEventHandler GetMODListAllCompleted;
        
        /// <remarks/>
        public event GetRCResultCompletedEventHandler GetRCResultCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetRuleResultByCustom", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetRuleResultByCustom(string modelname, string xmldatasource) {
            object[] results = this.Invoke("GetRuleResultByCustom", new object[] {
                        modelname,
                        xmldatasource});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomAsync(string modelname, string xmldatasource) {
            this.GetRuleResultByCustomAsync(modelname, xmldatasource, null);
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomAsync(string modelname, string xmldatasource, object userState) {
            if ((this.GetRuleResultByCustomOperationCompleted == null)) {
                this.GetRuleResultByCustomOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetRuleResultByCustomOperationCompleted);
            }
            this.InvokeAsync("GetRuleResultByCustom", new object[] {
                        modelname,
                        xmldatasource}, this.GetRuleResultByCustomOperationCompleted, userState);
        }
        
        private void OnGetRuleResultByCustomOperationCompleted(object arg) {
            if ((this.GetRuleResultByCustomCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetRuleResultByCustomCompleted(this, new GetRuleResultByCustomCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetRuleResultByCustomWithIdentityNo", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetRuleResultByCustomWithIdentityNo(string IdentityNo, string custName, string modelname, string xmldatasource) {
            object[] results = this.Invoke("GetRuleResultByCustomWithIdentityNo", new object[] {
                        IdentityNo,
                        custName,
                        modelname,
                        xmldatasource});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomWithIdentityNoAsync(string IdentityNo, string custName, string modelname, string xmldatasource) {
            this.GetRuleResultByCustomWithIdentityNoAsync(IdentityNo, custName, modelname, xmldatasource, null);
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomWithIdentityNoAsync(string IdentityNo, string custName, string modelname, string xmldatasource, object userState) {
            if ((this.GetRuleResultByCustomWithIdentityNoOperationCompleted == null)) {
                this.GetRuleResultByCustomWithIdentityNoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetRuleResultByCustomWithIdentityNoOperationCompleted);
            }
            this.InvokeAsync("GetRuleResultByCustomWithIdentityNo", new object[] {
                        IdentityNo,
                        custName,
                        modelname,
                        xmldatasource}, this.GetRuleResultByCustomWithIdentityNoOperationCompleted, userState);
        }
        
        private void OnGetRuleResultByCustomWithIdentityNoOperationCompleted(object arg) {
            if ((this.GetRuleResultByCustomWithIdentityNoCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetRuleResultByCustomWithIdentityNoCompleted(this, new GetRuleResultByCustomWithIdentityNoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetRuleResultByCustomWithIdentityNoCallFrom", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetRuleResultByCustomWithIdentityNoCallFrom(string CallFrom, string IdentityNo, string custName, string modelname, string xmldatasource) {
            object[] results = this.Invoke("GetRuleResultByCustomWithIdentityNoCallFrom", new object[] {
                        CallFrom,
                        IdentityNo,
                        custName,
                        modelname,
                        xmldatasource});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomWithIdentityNoCallFromAsync(string CallFrom, string IdentityNo, string custName, string modelname, string xmldatasource) {
            this.GetRuleResultByCustomWithIdentityNoCallFromAsync(CallFrom, IdentityNo, custName, modelname, xmldatasource, null);
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomWithIdentityNoCallFromAsync(string CallFrom, string IdentityNo, string custName, string modelname, string xmldatasource, object userState) {
            if ((this.GetRuleResultByCustomWithIdentityNoCallFromOperationCompleted == null)) {
                this.GetRuleResultByCustomWithIdentityNoCallFromOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetRuleResultByCustomWithIdentityNoCallFromOperationCompleted);
            }
            this.InvokeAsync("GetRuleResultByCustomWithIdentityNoCallFrom", new object[] {
                        CallFrom,
                        IdentityNo,
                        custName,
                        modelname,
                        xmldatasource}, this.GetRuleResultByCustomWithIdentityNoCallFromOperationCompleted, userState);
        }
        
        private void OnGetRuleResultByCustomWithIdentityNoCallFromOperationCompleted(object arg) {
            if ((this.GetRuleResultByCustomWithIdentityNoCallFromCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetRuleResultByCustomWithIdentityNoCallFromCompleted(this, new GetRuleResultByCustomWithIdentityNoCallFromCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetRuleResultByCustomAndVersion", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetRuleResultByCustomAndVersion(string identityNo, string custName, string modelname, string xmldatasource, string mainver, string subver) {
            object[] results = this.Invoke("GetRuleResultByCustomAndVersion", new object[] {
                        identityNo,
                        custName,
                        modelname,
                        xmldatasource,
                        mainver,
                        subver});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomAndVersionAsync(string identityNo, string custName, string modelname, string xmldatasource, string mainver, string subver) {
            this.GetRuleResultByCustomAndVersionAsync(identityNo, custName, modelname, xmldatasource, mainver, subver, null);
        }
        
        /// <remarks/>
        public void GetRuleResultByCustomAndVersionAsync(string identityNo, string custName, string modelname, string xmldatasource, string mainver, string subver, object userState) {
            if ((this.GetRuleResultByCustomAndVersionOperationCompleted == null)) {
                this.GetRuleResultByCustomAndVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetRuleResultByCustomAndVersionOperationCompleted);
            }
            this.InvokeAsync("GetRuleResultByCustomAndVersion", new object[] {
                        identityNo,
                        custName,
                        modelname,
                        xmldatasource,
                        mainver,
                        subver}, this.GetRuleResultByCustomAndVersionOperationCompleted, userState);
        }
        
        private void OnGetRuleResultByCustomAndVersionOperationCompleted(object arg) {
            if ((this.GetRuleResultByCustomAndVersionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetRuleResultByCustomAndVersionCompleted(this, new GetRuleResultByCustomAndVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetNetCreditPara", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataTable GetNetCreditPara(string certNo, string reportSN) {
            object[] results = this.Invoke("GetNetCreditPara", new object[] {
                        certNo,
                        reportSN});
            return ((System.Data.DataTable)(results[0]));
        }
        
        /// <remarks/>
        public void GetNetCreditParaAsync(string certNo, string reportSN) {
            this.GetNetCreditParaAsync(certNo, reportSN, null);
        }
        
        /// <remarks/>
        public void GetNetCreditParaAsync(string certNo, string reportSN, object userState) {
            if ((this.GetNetCreditParaOperationCompleted == null)) {
                this.GetNetCreditParaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetNetCreditParaOperationCompleted);
            }
            this.InvokeAsync("GetNetCreditPara", new object[] {
                        certNo,
                        reportSN}, this.GetNetCreditParaOperationCompleted, userState);
        }
        
        private void OnGetNetCreditParaOperationCompleted(object arg) {
            if ((this.GetNetCreditParaCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetNetCreditParaCompleted(this, new GetNetCreditParaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetMODListAll", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public MOD[] GetMODListAll(string isactive, int ctid, string ctname) {
            object[] results = this.Invoke("GetMODListAll", new object[] {
                        isactive,
                        ctid,
                        ctname});
            return ((MOD[])(results[0]));
        }
        
        /// <remarks/>
        public void GetMODListAllAsync(string isactive, int ctid, string ctname) {
            this.GetMODListAllAsync(isactive, ctid, ctname, null);
        }
        
        /// <remarks/>
        public void GetMODListAllAsync(string isactive, int ctid, string ctname, object userState) {
            if ((this.GetMODListAllOperationCompleted == null)) {
                this.GetMODListAllOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetMODListAllOperationCompleted);
            }
            this.InvokeAsync("GetMODListAll", new object[] {
                        isactive,
                        ctid,
                        ctname}, this.GetMODListAllOperationCompleted, userState);
        }
        
        private void OnGetMODListAllOperationCompleted(object arg) {
            if ((this.GetMODListAllCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetMODListAllCompleted(this, new GetMODListAllCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetRCResult", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetRCResult(string moduleName, int bid, int modId) {
            object[] results = this.Invoke("GetRCResult", new object[] {
                        moduleName,
                        bid,
                        modId});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetRCResultAsync(string moduleName, int bid, int modId) {
            this.GetRCResultAsync(moduleName, bid, modId, null);
        }
        
        /// <remarks/>
        public void GetRCResultAsync(string moduleName, int bid, int modId, object userState) {
            if ((this.GetRCResultOperationCompleted == null)) {
                this.GetRCResultOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetRCResultOperationCompleted);
            }
            this.InvokeAsync("GetRCResult", new object[] {
                        moduleName,
                        bid,
                        modId}, this.GetRCResultOperationCompleted, userState);
        }
        
        private void OnGetRCResultOperationCompleted(object arg) {
            if ((this.GetRCResultCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetRCResultCompleted(this, new GetRCResultCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class MOD {
        
        private int mOD_IDField;
        
        private string mOD_NMField;
        
        private int cT_IDField;
        
        private int mOD_STATField;
        
        private string mOD_VERField;
        
        private string mOD_SUBVERField;
        
        private string mOD_VER_GPField;
        
        private string mOD_DESCField;
        
        private System.DateTime mOD_CTField;
        
        private System.DateTime mOD_UTField;
        
        private int mOD_CUField;
        
        /// <remarks/>
        public int MOD_ID {
            get {
                return this.mOD_IDField;
            }
            set {
                this.mOD_IDField = value;
            }
        }
        
        /// <remarks/>
        public string MOD_NM {
            get {
                return this.mOD_NMField;
            }
            set {
                this.mOD_NMField = value;
            }
        }
        
        /// <remarks/>
        public int CT_ID {
            get {
                return this.cT_IDField;
            }
            set {
                this.cT_IDField = value;
            }
        }
        
        /// <remarks/>
        public int MOD_STAT {
            get {
                return this.mOD_STATField;
            }
            set {
                this.mOD_STATField = value;
            }
        }
        
        /// <remarks/>
        public string MOD_VER {
            get {
                return this.mOD_VERField;
            }
            set {
                this.mOD_VERField = value;
            }
        }
        
        /// <remarks/>
        public string MOD_SUBVER {
            get {
                return this.mOD_SUBVERField;
            }
            set {
                this.mOD_SUBVERField = value;
            }
        }
        
        /// <remarks/>
        public string MOD_VER_GP {
            get {
                return this.mOD_VER_GPField;
            }
            set {
                this.mOD_VER_GPField = value;
            }
        }
        
        /// <remarks/>
        public string MOD_DESC {
            get {
                return this.mOD_DESCField;
            }
            set {
                this.mOD_DESCField = value;
            }
        }
        
        /// <remarks/>
        public System.DateTime MOD_CT {
            get {
                return this.mOD_CTField;
            }
            set {
                this.mOD_CTField = value;
            }
        }
        
        /// <remarks/>
        public System.DateTime MOD_UT {
            get {
                return this.mOD_UTField;
            }
            set {
                this.mOD_UTField = value;
            }
        }
        
        /// <remarks/>
        public int MOD_CU {
            get {
                return this.mOD_CUField;
            }
            set {
                this.mOD_CUField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetRuleResultByCustomCompletedEventHandler(object sender, GetRuleResultByCustomCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetRuleResultByCustomCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetRuleResultByCustomCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetRuleResultByCustomWithIdentityNoCompletedEventHandler(object sender, GetRuleResultByCustomWithIdentityNoCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetRuleResultByCustomWithIdentityNoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetRuleResultByCustomWithIdentityNoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetRuleResultByCustomWithIdentityNoCallFromCompletedEventHandler(object sender, GetRuleResultByCustomWithIdentityNoCallFromCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetRuleResultByCustomWithIdentityNoCallFromCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetRuleResultByCustomWithIdentityNoCallFromCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetRuleResultByCustomAndVersionCompletedEventHandler(object sender, GetRuleResultByCustomAndVersionCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetRuleResultByCustomAndVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetRuleResultByCustomAndVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetNetCreditParaCompletedEventHandler(object sender, GetNetCreditParaCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetNetCreditParaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetNetCreditParaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Data.DataTable Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Data.DataTable)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetMODListAllCompletedEventHandler(object sender, GetMODListAllCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetMODListAllCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetMODListAllCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public MOD[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((MOD[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void GetRCResultCompletedEventHandler(object sender, GetRCResultCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetRCResultCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetRCResultCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591