using System;
using System.Collections.Generic;
using NHibernate.Util;

namespace NHibernate.Mapping
{
	/// <summary>
	/// The mapping for a component, composite element, composite identifier,
	/// etc.
	/// </summary>
	public class Component : SimpleValue
	{
		private readonly List<Property> properties = new List<Property>();
		private System.Type componentClass;
		private bool embedded;
		private string parentProperty;
		private PersistentClass owner;
		private bool dynamic;
		private bool isKey;
		private string roleName;
		private Dictionary<EntityMode, string> tuplizerImpls;

		/// <summary></summary>
		public int PropertySpan
		{
			get { return properties.Count; }
		}

		/// <summary></summary>
		public IEnumerable<Property> PropertyIterator
		{
			get { return properties; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="p"></param>
		public void AddProperty(Property p)
		{
			properties.Add(p);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="column"></param>
		public override void AddColumn(Column column)
		{
			throw new NotSupportedException("Cant add a column to a component");
		}

		/// <summary></summary>
		public override int ColumnSpan
		{
			get
			{
				int n = 0;
				foreach (Property p in PropertyIterator)
				{
					n += p.ColumnSpan;
				}
				return n;
			}
		}

		/// <summary></summary>
		public override IEnumerable<ISelectable> ColumnIterator
		{
			get
			{
				List<IEnumerable<ISelectable>> iters = new List<IEnumerable<ISelectable>>();
				foreach (Property property in PropertyIterator)
				{
					iters.Add(property.ColumnIterator);
				}
				return new JoinedEnumerable<ISelectable>(iters);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner"></param>
		public Component(PersistentClass owner)
			: base(owner.Table)
		{
			this.owner = owner;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="table"></param>
		public Component(Table table)
			: base(table)
		{
			this.owner = null;
		}

		public Component(Join join)
			: base(join.Table)
		{
			this.owner = join.PersistentClass;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="propertyClass"></param>
		/// <param name="propertyName"></param>
		/// <param name="propertyAccess"></param>
		public override void SetTypeByReflection(System.Type propertyClass, string propertyName, string propertyAccess)
		{
		}

		/// <summary></summary>
		public bool IsEmbedded
		{
			get { return embedded; }
			set { embedded = value; }
		}

		/// <summary></summary>
		public bool IsDynamic
		{
			get { return dynamic; }
			set { dynamic = value; }
		}

		/// <summary></summary>
		public System.Type ComponentClass
		{
			get { return componentClass; }
			set { componentClass = value; }
		}

		/// <summary></summary>
		public PersistentClass Owner
		{
			get { return owner; }
			set { owner = value; }
		}

		/// <summary></summary>
		public string ParentProperty
		{
			get { return parentProperty; }
			set { parentProperty = value; }
		}

		public override bool[] ColumnInsertability
		{
			get
			{
				bool[] result = new bool[ColumnSpan];
				int i = 0;
				foreach (Property prop in PropertyIterator)
				{
					bool[] chunk = prop.Value.ColumnInsertability;
					if (prop.IsInsertable)
					{
						System.Array.Copy(chunk, 0, result, i, chunk.Length);
					}
					i += chunk.Length;
				}
				return result;
			}
		}

		public override bool[] ColumnUpdateability
		{
			get
			{
				bool[] result = new bool[ColumnSpan];
				int i = 0;
				foreach (Property prop in PropertyIterator)
				{
					bool[] chunk = prop.Value.ColumnUpdateability;
					if (prop.IsUpdateable)
					{
						System.Array.Copy(chunk, 0, result, i, chunk.Length);
					}
					i += chunk.Length;
				}
				return result;
			}
		}

		public bool IsKey
		{
			get { return isKey; }
			set { isKey = value; }
		}

		public string RoleName
		{
			get { return roleName; }
			set { roleName = value; }
		}

		public Property GetProperty(string propertyName)
		{
			IEnumerable<Property> iter = PropertyIterator;
			foreach (Property prop in iter)
			{
				if (prop.Name.Equals(propertyName))
				{
					return prop;
				}
			}
			throw new MappingException("component property not found: " + propertyName);
		}

		public virtual void AddTuplizer(EntityMode entityMode, string implClassName)
		{
			if (tuplizerImpls == null)
				tuplizerImpls = new Dictionary<EntityMode, string>();

			tuplizerImpls[entityMode] = implClassName;
		}

		public virtual string GetTuplizerImplClassName(EntityMode mode)
		{
			// todo : remove this once ComponentMetamodel is complete and merged
			if (tuplizerImpls == null)
			{
				return null;
			}
			return tuplizerImpls[mode];
		}

		public virtual IDictionary<EntityMode, string> TuplizerMap
		{
			get
			{
				if (tuplizerImpls == null)
					return null;

				return tuplizerImpls;
			}
		}

		public bool HasPojoRepresentation
		{
			get { return componentClass != null; }
		}

	}
}
