using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.Xrm.Sdk;
using System.IdentityModel.Tokens;

public class CRMUserNameSecurityTokenHandler : Microsoft.IdentityModel.Tokens.UserNameSecurityTokenHandler
{
    public CRMUserNameSecurityTokenHandler()
    {

    }

    public override bool CanValidateToken
    {
        get
        {
            return true;

        }
    }

    public override Microsoft.IdentityModel.Claims.ClaimsIdentityCollection ValidateToken(SecurityToken token)
    {
        System.Diagnostics.Debugger.Launch();

        if (token == null)
        {
            throw new ArgumentNullException("token");
        }
        UserNameSecurityToken userNameToken = token as UserNameSecurityToken;

        if (userNameToken == null)
        {
            throw new SecurityTokenException("Invalid token");
        }
        IClaimsIdentity identity = new ClaimsIdentity();
        EntityCollection contacts = AuthenticateUser(userNameToken.UserName, userNameToken.Password);
        if (contacts != null && contacts.Entities.First() != null)
        {
            Entity contact = contacts.Entities.First();

            identity.Claims.Add(new Claim(Microsoft.IdentityModel.Claims.ClaimTypes.Authentication, "true"));
            identity.Claims.Add(new Claim(Microsoft.IdentityModel.Claims.ClaimTypes.Upn, userNameToken.UserName));
            string name = contact.Attributes["lastname"] != null ? contact.Attributes["lastname"].ToString() + ", " : "" +
                contact.Attributes["middlename"] != null ? contact.Attributes["middlename"].ToString() + " " : "" +
                contact.Attributes["firstname"] != null ? contact.Attributes["firstname"].ToString() : "";
            identity.Claims.Add(new Claim(Microsoft.IdentityModel.Claims.ClaimTypes.Name, name));
            identity.Claims.Add(new Claim(Microsoft.IdentityModel.Claims.ClaimTypes.Email, contact.Attributes["emailaddress1"] != null ? contact.Attributes["emailaddress1"].ToString() : userNameToken.UserName));
            identity.Claims.Add(new Claim(Microsoft.IdentityModel.Claims.ClaimTypes.PrimarySid, contact.Attributes["contactid"] != null ? contact.Attributes["contactid"].ToString() : "InvalidContactID"));
        }

        return new ClaimsIdentityCollection(new IClaimsIdentity[] { identity });
    }

    private EntityCollection AuthenticateUser(string userName, string password)
    {
        //Authentication against CRM
        CRMService svc = new CRMService();
        var fetchXML = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='contact'>
                                            <all-attributes/>
                                            <filter type='and'>
                                              <condition attribute='neu_username' operator='eq' value='" + userName + @"' />
                                            </filter>
                                          </entity>
                                        </fetch>";
        EntityCollection contacts = svc.RetreiveMultipleRecords(fetchXML);

        if (contacts.Entities.Count > 0)
        {
            if (password.ToLower() == contacts[0]["new_password_field"].ToString().ToLower())
                return contacts;
        }

        return null;
    }
}