Vereisten en aantekeningen DisplayConfig Entity en bijbehorende entities.
Doel
De DisplayConfig entity en bijbehorende entities zijn bedoeld om een weergave-pagina van models in de Knights and Kings systeem te configureren, net zoals de FormConfig dat doet voor het aanmaken van create/update forms. Doormiddel van een DisplayConfig kan een administrator configureren welke velden er van de entity en zijn bijbehorende relaties worden getoond, en eventuele groepering van de te tonen velden.
De DisplayConfig entities worden aangemaakt door administrators in een DisplayConfigBuilder component voor entities van Knights and Kings. Deze DisplayConfig entities zijn herbruikbaar en kunnen als component door de developer op diverse plekken in de web app ingezet worden. Hierdoor hoeft de developer niet voor elke use case een aparte display per entity te maken, maar kan de administrator dit eenvoudig vanuit de web app zelf doen, zonder updates van de web app.
De DisplayConfig lijkt op de FormConfig. Hij bevat net zoals de FormConfig Steps, al heten die bij de DisplayConfig Sections. Deze Sections zijn eigenlijk een verzameling van velden die bij elkaar in een blok getoont worden. De administrator kan in de DisplayConfigBuilder te tonen velden toevoegen en ordenen in welke volgorde ze getoond dienen te worden.
Het DisplayConfig ontwerp bevat in grote lijnen de volgende objecten die ontwikkeld dienen te worden: 
DisplayConfiguration
Bedoelt als template voor de frontend om te tonen informatie van entities te ordenen en in een configurabel ontwerp te displayen in een DisplayWizard component. Een entity uit Knights and Kings (zoals een Town) kan meerdere DisplayConfig instances hebben (met maximaal 1 default, net als bij de FormConfigs).
Bevat ten minste de volgende velden (veldnamen staan niet vast en kunnen indien nodig verandert worden):
-	Id (int) (required)
-	Name (string) (required, default = “${EntityTypeName} Display Template”)
-	EntityTypeName (string) (required) verwijst naar de class naam van de KnK entity (bijv. “Town”)
-	IsDefault (bool) (required, default = false)
-	Description (string) 
-	CreatedAt (datetime)
-	UpdatedAt (datetime)
-	SectionOrderJson (string) JSON array storing the ordered Ids of the DisplaySections
-	Sections (List<DisplaySection>) (default = new())
DisplaySection
Bedoelt als datagroep voor de frontend om de te tonen velden van de EntityTypeName te groeperen. Dit is handig voor bijvoorbeeld algemene datavelden van de Town entity. Ook kan je hiermee bijvoorbeeld data van een related entity groeperen zoals de Districts van een Town. Een belangrijke requirement voor de DisplaySection is dat deze toe te passen is op een Collection FieldType. Dit houdt in dat bijvoorbeeld Town entity een veld heeft genaamd Districts met als datatype Collection<District>. Dit is een ObjectType Collection, ElementType Object en ObjectType District (dit is hoe het in de FormField entity wordt beschreven in de FormConfiguration feature). Ik wil dat ik als administrator voor elke entry in deze collection een DisplaySection krijg die velden toont die ik instel in de DisplayConfigBuilder.
Ik wil als administrator in de DisplaySection velden van de EntityTypeName kunnen selecteren die getoont moeten worden in de DisplaySection. Net zoals ik velden kan selecteren in de FormConfigBuilder in de FormStep. Ook wil ik velden kunnen selecteren van related entities. In het geval van een Town entity die een veld heeft genaamd TownHouse met als datatype Structure, dan wil ik in de DisplaySection kunnen aangeven dat ik TownHouse getoond wil hebben, en vervolgens velden van de entity Structure kunnen selecteren zoals Id, Name, Street, StreetNumber. Het systeem moet hier dynamisch mee om kunnen gaan en ook werken voor andere entities en relaties dan beschreven in deze voorbeelden. Laat het systeem de te selecteren velden ophalen en tonen dmv. De EntityMetaData. De DisplaySection moet ook een Titel en een beschrijving hebben.
Een DisplaySection moet gekoppeld kunnen worden aan een related entity van de Target Entity van de DisplayConfig. Dit houdt in dat de section volledig toegewijd is aan deze related entity en de administrator velden van de related entity kan kiezen om de tonen. Ook kan de administrator bepaalde functies activeren of deactiveren voor de section, bijvoorbeeld een knop die de de DisplayWizard opent met de related entity, een knop om de related entity te wijzigen (FormWizard met defauklt Config te openen), een andere related entity selecteren voor de target entity voor dit relationship veld, de relatie volledig te verwijderen.

Bevat ten minste de volgende velden (veldnamen staan niet vast en kunnen indien nodig verandert worden):
-	Id (int)
-	SectionGuid (Guid) Unique identifier for this instance used in SectionOrderJson in de DisplayConfig
-	SectionName (string) (required)
-	Description (string)
-	IsReusable (bool) (default = false)
-	SourceSectionId (int) Tracks which reusable step template was used to create this step instance. NULL if this is an original step (not cloned from a template).
-	IsLinkedToSource (bool) (default = false) Indicates whether this step is linked to a source template (true) or is an independent copy (false).
/// 
/// Link mode (IsLinkedToSource = true):
/// - This step references the source template (SourceStepId).
/// - Display properties (StepName, Description, FieldOrderJson) are loaded from the source at read-time.
/// - Changes to the source template are immediately visible in linked instances.
/// - Limited to FieldOrderJson and field order for customization.
/// 
/// Copy mode (IsLinkedToSource = false):
/// - This step is a full clone of the source, fully independent after creation.
/// - All properties (StepName, Description, FieldOrderJson) are owned by this instance.
/// - Changes to the source template do NOT affect this copy.
/// - SourceStepId is kept only for traceability/analytics purposes.
/// 
/// Default: false (copy mode is the standard behavior).
-	FieldOrderJson (string)         /// <summary>
/// JSON array storing the ordered GUIDs of fields: ["field-guid-1", "field-guid-2", ...].
/// 
/// WHY JSON for field ordering?
/// Same reasons as StepOrderJson in FormConfiguration:
/// 1. Easy reordering without updating multiple rows.
/// 2. Supports dynamic field insertion/removal.
/// 3. Each step instance can have its own field order (important for copy-on-reuse pattern).
/// 4. Simplifies API: Send entire step with ordered field list in one JSON payload.
/// 
/// Example scenario:
/// - Reusable step "Basic Info" has fields: [Name, Description, Category].
/// - Config A uses it as: [Name, Category, Description] (reordered).
/// - Config B uses it as: [Category, Name, Description] (different order).
/// Since each config clones the step, each has its own FieldOrderJson.
/// </summary>
-	CreatedAt (datetime)
-	UpdatedAt (datetime)
-	DisplayConfigurationId (int)
-	DisplayConfiguration (DisplayConfiguration)
-	Fields (DisplayFields)

DisplayField
De DisplayField is net als de FormField, alleen heeft de DisplayField geen inputfield wanneer deze getoond wordt. In een DisplayField kan de administrator selecteren welk veld er van de target entity of related entity getoond wordt. Een belangrijke eis van de DisplayField is dat de administrator een tekst kan opstellen met variabelen, net zoals `Deze tekst bevat ${variables.count} aantal variabelen van de entity ${entity} en related entity ${entity.TownHouse.Name}`, maar dan in een meer gebruiksvriendelijk jasje.
Bevat ten minste de volgende velden (veldnamen staan niet vast en kunnen indien nodig verandert worden):
-	Id (int)
-	FieldGuid (Guid) Unique identifier for this field instance. Used in FieldOrderJson of the DisplaySection entity to reference this specific field.
-	FieldName (string)
-	Label (string)
-	Placeholder (string)
-	Description (string)
-	FieldName (string)

Kanttekeningen en opmerkingen
1.	De reusable feature van de FormConfiguration’s FormStep en FormField moet ook geïmplementeerd worden bij de DisplaySection.
