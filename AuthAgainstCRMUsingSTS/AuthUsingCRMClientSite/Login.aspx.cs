//-----------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
//-----------------------------------------------------------------------------

using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Web.Configuration;
using System.Web.Security;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens;
using MIB = Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using SSS = System.ServiceModel.Security;

public partial class Login : System.Web.UI.Page
{ 
    protected void btnSubmit_Click( object sender, EventArgs e )
    {
        // Note: Add code to validate user name, password. This code is for illustrative purpose only.
        // Do not use it in production environment.        
        //FormsAuthentication.RedirectFromLoginPage( txtUserName.Text, false );

        GetToken();
    }

    private static void GetToken()
    {
        WSTrustChannelFactory factory = null;
        try
        {
            factory = new WSTrustChannelFactory(
                            new MIB.UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential),//, HttpClientCredentialType.None),
                            new EndpointAddress("https://apandu01.corp.neudesic.net/AuthUsingCRM/Service.svc/IWSTrust13")); //IWSTrust13

            factory.TrustVersion = SSS.TrustVersion.WSTrust13;
            factory.Credentials.SupportInteractive = false;
            factory.Credentials.UserName.UserName = "corp\\manoj.kumar";
            factory.Credentials.UserName.Password = "jonam*33";

            factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = SSS.X509CertificateValidationMode.None;
            factory.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            factory.Credentials.ClientCertificate.Certificate = CertificateUtil.GetCertificate(StoreName.My, StoreLocation.LocalMachine, WebConfigurationManager.AppSettings["SigningCertificateName"]);

            RequestSecurityToken rst = new RequestSecurityToken();
            rst.RequestType = RequestTypes.Issue;

            rst.AppliesTo = new EndpointAddress("https://apandu01.corp.neudesic.net/AuthUsingCRMClientSite/");
            rst.KeyType = KeyTypes.Bearer;

            IWSTrustChannelContract channel = factory.CreateChannel();

            SecurityToken secToken = channel.Issue(rst);

            // SamlSecurityToken samlToken = secToken as SamlSecurityToken;
        }
        finally
        {
            if (factory.State != CommunicationState.Closing)
            {
                factory.Close();
                factory.Abort();
            }
        }
    }
}
