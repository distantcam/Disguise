using Mono.Cecil;
using Mono.Cecil.Rocks;
using TiviT.NCloak.Mapping;

namespace TiviT.NCloak.CloakTasks
{
    public class MappingTask : ICloakTask
    {
        /// <summary>
        /// Gets the task name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Creating call map"; }
        }

        /// <summary>
        /// Runs the specified cloaking task.
        /// </summary>
        /// <param name="context">The running context of this cloak job.</param>
        public void RunTask(CloakContext context)
        {
            //Get out if rename is turned off
            if (context.Settings.NoRename)
                return;

            //Go through the members and build up a mapping graph
            //If this is done then the members in the graph will be obfuscated, otherwise we'll
            //just obfuscate private members

            //Set up the mapping graph
            AssemblyMapping assemblyMapping = context.MappingGraph.AddAssembly(context.AssemblyDefinition);

            //Get a reference to the name manager
            NameManager nameManager = context.NameManager;

            //Go through each module
            foreach (ModuleDefinition moduleDefinition in context.AssemblyDefinition.Modules)
            {
                //Go through each type
                foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes())
                {
                    //First of all - see if we've declared it already - if so get the existing reference
                    TypeMapping typeMapping = assemblyMapping.GetTypeMapping(typeDefinition);
                    if (typeMapping == null)
                    {
                        //We don't have it - get it
                        if (context.Settings.ObfuscateAllModifiers)
                            typeMapping = assemblyMapping.AddType(typeDefinition,
                                                                  nameManager.GenerateName(NamingTable.Type));
                        else
                            typeMapping = assemblyMapping.AddType(typeDefinition, null);
                    }

                    //Go through each method
                    foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                    {
                        MapMethod(context, assemblyMapping, nameManager, typeDefinition, typeMapping, methodDefinition);
                    }

                    //Properties
                    foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties)
                    {
                        MapProperty(context, assemblyMapping, nameManager, typeDefinition, typeMapping, propertyDefinition);
                    }

                    //Fields
                    foreach (FieldDefinition fieldDefinition in typeDefinition.Fields)
                    {
                        MapField(context, nameManager, typeMapping, fieldDefinition);
                    }
                }
            }
        }

        private static void MapMethod(CloakContext context, AssemblyMapping assemblyMapping, NameManager nameManager, TypeDefinition typeDefinition, TypeMapping typeMapping, MethodDefinition methodDefinition)
        {
            //First of all - check if we've obfuscated it already - if we have then don't bother
            if (typeMapping.HasMethodBeenObfuscated(methodDefinition.Name))
                return;

            //We won't do constructors - causes issues
            if (methodDefinition.IsConstructor)
                return;

            //We haven't - let's work out the obfuscated name
            if (context.Settings.ObfuscateAllModifiers)
            {
                //Take into account whether this is overriden, or an interface implementation
                if (methodDefinition.IsVirtual)
                {
                    //We handle this differently - rather than creating a new name each time we need to reuse any already generated names
                    //We do this by firstly finding the root interface or object
                    TypeDefinition baseType = FindBaseTypeDeclaration(typeDefinition, methodDefinition);
                    if (baseType != null)
                    {
                        //Find it in the mappings
                        TypeMapping baseTypeMapping = assemblyMapping.GetTypeMapping(baseType);
                        if (baseTypeMapping != null)
                        {
                            //We found the type mapping - look up the name it uses for this method and use that
                            if (baseTypeMapping.HasMethodMapping(methodDefinition))
                                typeMapping.AddMethodMapping(methodDefinition, baseTypeMapping.GetObfuscatedMethodName(methodDefinition));
                            else
                            {
                                //That's strange... we shouldn't get into here - but if we ever do then
                                //we'll add the type mapping into both
                                string obfuscatedName = nameManager.GenerateName(NamingTable.Method);
                                typeMapping.AddMethodMapping(methodDefinition, obfuscatedName);
                                baseTypeMapping.AddMethodMapping(methodDefinition, obfuscatedName);
                            }
                        }
                        else
                        {
                            //Otherwise add it into our list manually
                            //at the base level first off
                            baseTypeMapping = assemblyMapping.AddType(baseType,
                                                      nameManager.GenerateName(NamingTable.Type));
                            string obfuscatedName = nameManager.GenerateName(NamingTable.Method);
                            baseTypeMapping.AddMethodMapping(methodDefinition, obfuscatedName);
                            //Now at our implemented level
                            typeMapping.AddMethodMapping(methodDefinition, obfuscatedName);
                        }
                    }
                    else
                    {
                        //We must be at the base already - add normally
                        typeMapping.AddMethodMapping(methodDefinition,
                                             nameManager.GenerateName(NamingTable.Method));
                    }
                }
                else //Add normally
                    typeMapping.AddMethodMapping(methodDefinition,
                                             nameManager.GenerateName(NamingTable.Method));
            }
            else if (methodDefinition.IsPrivate)
                typeMapping.AddMethodMapping(methodDefinition, nameManager.GenerateName(NamingTable.Method));
        }

        private static void MapProperty(CloakContext context, AssemblyMapping assemblyMapping, NameManager nameManager, TypeDefinition typeDefinition, TypeMapping typeMapping, PropertyDefinition propertyDefinition)
        {
            //First of all - check if we've obfuscated it already - if we have then don't bother
            if (typeMapping.HasPropertyBeenObfuscated(propertyDefinition.Name))
                return;

            //Go through the old fashioned way
            if (context.Settings.ObfuscateAllModifiers)
            {
                if ((propertyDefinition.GetMethod != null && propertyDefinition.GetMethod.IsVirtual) ||
                    (propertyDefinition.SetMethod != null && propertyDefinition.SetMethod.IsVirtual))
                {
                    //We handle this differently - rather than creating a new name each time we need to reuse any already generated names
                    //We do this by firstly finding the root interface or object
                    TypeDefinition baseType = FindBaseTypeDeclaration(typeDefinition, propertyDefinition);
                    if (baseType != null)
                    {
                        //Find it in the mappings
                        TypeMapping baseTypeMapping = assemblyMapping.GetTypeMapping(baseType);
                        if (baseTypeMapping != null)
                        {
                            //We found the type mapping - look up the name it uses for this property and use that
                            if (baseTypeMapping.HasPropertyMapping(propertyDefinition))
                                typeMapping.AddPropertyMapping(propertyDefinition, baseTypeMapping.GetObfuscatedPropertyName(propertyDefinition));
                            else
                            {
                                //That's strange... we shouldn't get into here - but if we ever do then
                                //we'll add the type mapping into both
                                string obfuscatedName = nameManager.GenerateName(NamingTable.Property);
                                typeMapping.AddPropertyMapping(propertyDefinition, obfuscatedName);
                                baseTypeMapping.AddPropertyMapping(propertyDefinition, obfuscatedName);
                            }
                        }
                        else
                        {
                            //Otherwise add it into our list manually
                            //at the base level first off
                            baseTypeMapping = assemblyMapping.AddType(baseType,
                                                      nameManager.GenerateName(NamingTable.Type));
                            string obfuscatedName = nameManager.GenerateName(NamingTable.Property);
                            baseTypeMapping.AddPropertyMapping(propertyDefinition, obfuscatedName);
                            //Now at our implemented level
                            typeMapping.AddPropertyMapping(propertyDefinition, obfuscatedName);
                        }
                    }
                    else
                    {
                        //We must be at the base already - add normally
                        typeMapping.AddPropertyMapping(propertyDefinition,
                                             nameManager.GenerateName(NamingTable.Property));
                    }
                }
                else
                    typeMapping.AddPropertyMapping(propertyDefinition,
                                               nameManager.GenerateName(NamingTable.Property));
            }
            else if (propertyDefinition.GetMethod != null && propertyDefinition.SetMethod != null)
            {
                //Both parts need to be private
                if (propertyDefinition.GetMethod.IsPrivate && propertyDefinition.SetMethod.IsPrivate)
                    typeMapping.AddPropertyMapping(propertyDefinition, nameManager.GenerateName(NamingTable.Property));
            }
            else if (propertyDefinition.GetMethod != null)
            {
                //Only the get is present - make sure it is private
                if (propertyDefinition.GetMethod.IsPrivate)
                    typeMapping.AddPropertyMapping(propertyDefinition, nameManager.GenerateName(NamingTable.Property));
            }
            else if (propertyDefinition.SetMethod != null)
            {
                //Only the set is present - make sure it is private
                if (propertyDefinition.SetMethod.IsPrivate)
                    typeMapping.AddPropertyMapping(propertyDefinition, nameManager.GenerateName(NamingTable.Property));
            }
        }

        private static void MapField(CloakContext context, NameManager nameManager, TypeMapping typeMapping, FieldDefinition fieldDefinition)
        {
            //First of all - check if we've obfuscated it already - if we have then don't bother
            if (typeMapping.HasFieldBeenObfuscated(fieldDefinition.Name))
                return;

            if (context.Settings.ObfuscateAllModifiers)
                typeMapping.AddFieldMapping(fieldDefinition, nameManager.GenerateName(NamingTable.Field));
            else if (fieldDefinition.IsPrivate)
            {
                //Rename if private
                typeMapping.AddFieldMapping(fieldDefinition, nameManager.GenerateName(NamingTable.Field));
            }
        }

        private static TypeDefinition FindBaseTypeDeclaration(TypeDefinition definition, MethodReference method)
        {
            //Search the interfaces first
            foreach (TypeReference tr in definition.Interfaces)
            {
                //Convert to a type definition
                TypeDefinition td = tr.GetTypeDefinition();
                MethodDefinition md = td.Methods.FindMethod(method.Name, method.Parameters);
                if (md != null)
                    return td;

                //Do a recursive search below
                TypeDefinition baseInterface = FindBaseTypeDeclaration(td, method);
                if (baseInterface != null)
                    return baseInterface;
            }

            //Search the base class
            TypeReference baseTr = definition.BaseType;
            if (baseTr != null)
            {
                TypeDefinition baseTd = baseTr.GetTypeDefinition();
                if (baseTd != null)
                {
                    MethodDefinition md = baseTd.Methods.FindMethod(method.Name, method.Parameters);
                    if (md != null)
                        return baseTd;

                    //Do a recursive search below
                    TypeDefinition baseClass = FindBaseTypeDeclaration(baseTd, method);
                    if (baseClass != null)
                        return baseClass;
                }
            }

            //We've exhausted all options
            return null;
        }

        private static TypeDefinition FindBaseTypeDeclaration(TypeDefinition definition, MemberReference property)
        {
            //Search the interfaces first
            foreach (TypeReference tr in definition.Interfaces)
            {
                //Convert to a type definition
                TypeDefinition td = tr.GetTypeDefinition();
                if (td.Properties.HasProperty(property.Name))
                    return td;

                //Do a recursive search below
                TypeDefinition baseInterface = FindBaseTypeDeclaration(td, property);
                if (baseInterface != null)
                    return baseInterface;
            }

            //Search the base class
            TypeReference baseTr = definition.BaseType;
            if (baseTr != null)
            {
                TypeDefinition baseTd = baseTr.GetTypeDefinition();
                if (baseTd != null)
                {
                    if (baseTd.Properties.HasProperty(property.Name))
                        return baseTd;

                    //Do a recursive search below
                    TypeDefinition baseClass = FindBaseTypeDeclaration(baseTd, property);
                    if (baseClass != null)
                        return baseClass;
                }
            }

            //We've exhausted all options
            return null;
        }
    }
}