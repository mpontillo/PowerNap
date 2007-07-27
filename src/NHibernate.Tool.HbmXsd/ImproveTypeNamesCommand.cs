using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace NHibernate.Tool.HbmXsd
{
	/// <summary>
	/// Responsible for improving the type names in code generated by an <see cref="XsdCodeGenerator" />.
	/// </summary>
	public class ImproveTypeNamesCommand
	{
		private readonly Dictionary<string, string> changedTypeNames = new Dictionary<string, string>();
		private readonly CodeNamespace code;

		public ImproveTypeNamesCommand(CodeNamespace code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

			this.code = code;
		}

		/// <summary>Changes type names to use camel casing.</summary>
		public void Execute()
		{
			ChangeDeclaredTypeNames();
			UpdateTypeReferences();
		}

		private void ChangeDeclaredTypeNames()
		{
			foreach (CodeTypeDeclaration type in code.Types)
			{
				string rootElementName = GetRootElementName(type);
				string newTypeName = GetNewTypeName(type.Name, rootElementName);
				changedTypeNames[type.Name] = newTypeName;
				type.Name = newTypeName;
			}
		}

		private void UpdateTypeReferences()
		{
			foreach (CodeTypeDeclaration type in code.Types)
				foreach (CodeTypeMember member in type.Members)
				{
					CodeMemberField field = member as CodeMemberField;
					CodeConstructor constructor = member as CodeConstructor;

					if (field != null)
						UpdateFieldTypeReferences(field);

					else if (constructor != null)
						UpdateMethodTypeReferences(constructor);
				}
		}

		private static string GetRootElementName(CodeTypeMember type)
		{
			foreach (CodeAttributeDeclaration attribute in type.CustomAttributes)
				if (attribute.Name == typeof (XmlRootAttribute).FullName)
					foreach (CodeAttributeArgument argument in attribute.Arguments)
						if (argument.Name == "")
							return ((CodePrimitiveExpression) argument.Value).Value.ToString();

			return null;
		}

		protected virtual string GetNewTypeName(string originalName, string rootElementName)
		{
			if (rootElementName == null)
				return StringTools.CamelCase(originalName);
			else
				return StringTools.CamelCase(rootElementName);
		}

		private void UpdateFieldTypeReferences(CodeMemberField field)
		{
			if (field.Type.ArrayElementType != null)
				UpdateTypeReference(field.Type.ArrayElementType);
			else
				UpdateTypeReference(field.Type);

			foreach (CodeAttributeDeclaration attribute in field.CustomAttributes)
				if (attribute.Name == typeof (XmlElementAttribute).FullName)
				{
					if (attribute.Arguments.Count == 2)
						UpdateTypeReference(((CodeTypeOfExpression) attribute.Arguments[1].Value).Type);
				}
				else if (attribute.Name == typeof (DefaultValueAttribute).FullName)
				{
					CodeFieldReferenceExpression reference = attribute.Arguments[0].Value
						as CodeFieldReferenceExpression;

					if (reference != null)
						UpdateTypeReference(((CodeTypeReferenceExpression) reference.TargetObject).Type);
				}
		}

		private void UpdateMethodTypeReferences(CodeMemberMethod method)
		{
			foreach (CodeStatement statement in method.Statements)
			{
				CodeAssignStatement assignment = (CodeAssignStatement) statement;
				CodeFieldReferenceExpression right = assignment.Right as CodeFieldReferenceExpression;

				if (right != null)
					UpdateTypeReference(((CodeTypeReferenceExpression) right.TargetObject).Type);
			}
		}

		private void UpdateTypeReference(CodeTypeReference type)
		{
			if (changedTypeNames.ContainsKey(type.BaseType))
				type.BaseType = changedTypeNames[type.BaseType];
		}
	}
}