using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Client;

public class CRMService
{
    private OrganizationService _Service = null;

    public CRMService()
    {
        var connection = new CrmConnection("CRMConn");
        _Service = new OrganizationService(connection);
    }

    public OrganizationService Service
    {
        get
        {
            return _Service;
        }
        set
        {

            _Service = value;
        }
    }

    public Guid CreateEntity(Entity entity)
    {
        return Service.Create(entity);
    }

    public Entity RetreiveEntity(Guid id, string entityName)
    {
        return Service.Retrieve(entityName, id, new ColumnSet(true));
    }

    public EntityCollection RetreiveMultipleRecords(string fetchXml)
    {
        return Service.RetrieveMultiple(new FetchExpression(fetchXml));
    }

    public void UpdateEntity(Entity entity)
    {
        Service.Update(entity);
    }
}