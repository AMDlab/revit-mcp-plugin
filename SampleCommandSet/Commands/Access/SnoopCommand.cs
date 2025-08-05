// SnoopCommand.cs
using System.Collections.Generic;
using System.Xml.Linq;

public class SnoopCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData,
                         ref string message,
                         ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var selection = commandData.Application.ActiveUIDocument.Selection;

        var selectedIds = selection.GetElementIds();
        var snoopResults = new List<ElementSnoopData>();

        foreach (var id in selectedIds)
        {
            var element = doc.GetElement(id);
            var snoopData = SnoopElement(element);
            snoopResults.Add(snoopData);
        }

        // JSON形式で返す
        return JsonConvert.SerializeObject(snoopResults);
    }

    private ElementSnoopData SnoopElement(Element element)
    {
        var data = new ElementSnoopData
        {
            Id = element.Id.IntegerValue,
            Category = element.Category?.Name,
            TypeName = element.GetType().Name,
            Parameters = GetAllParameters(element),
            Geometry = GetGeometryInfo(element),
            Relationships = GetRelationships(element)
        };

        return data;
    }

    private Dictionary<string, object> GetAllParameters(Element element)
    {
        var parameters = new Dictionary<string, object>();

        // インスタンスパラメータ
        foreach (Parameter param in element.Parameters)
        {
            parameters[param.Definition.Name] = GetParameterValue(param);
        }

        // タイプパラメータ
        if (element is FamilyInstance fi && fi.Symbol != null)
        {
            foreach (Parameter param in fi.Symbol.Parameters)
            {
                parameters[$"Type.{param.Definition.Name}"] = GetParameterValue(param);
            }
        }

        return parameters;
    }
}