using System;

namespace NHibernate.Property
{
	/// <summary>
	/// Access the mapped property through a Property <c>get</c> to get the value 
	/// and go directly to the Field to set the value.
	/// </summary>
	/// <remarks>
	/// This is most useful because Classes can provider a get for the Property
	/// that is the &lt;id&gt; but tell NHibernate there is no setter for the Property
	/// so the value should be written directly to the field.
	/// </remarks>
	public class NoSetterAccessor : IPropertyAccessor
	{
		private IFieldNamingStrategy namingStrategy;

		/// <summary>
		/// Initializes a new instance of <see cref="NoSetterAccessor"/>.
		/// </summary>
		/// <param name="namingStrategy">The <see cref="IFieldNamingStrategy"/> to use.</param>
		public NoSetterAccessor( IFieldNamingStrategy namingStrategy )
		{
			this.namingStrategy = namingStrategy;
		}

		#region IPropertyAccessor Members

		/// <summary>
		/// Creates an <see cref="BasicGetter"/> to <c>get</c> the value from the Property.
		/// </summary>
		/// <param name="theClass">The <see cref="System.Type"/> to find the Property in.</param>
		/// <param name="propertyName">The name of the Property to get.</param>
		/// <returns>
		/// The <see cref="BasicGetter"/> to use to get the value of the Property from an
		/// instance of the <see cref="System.Type"/>.</returns>
		/// <exception cref="PropertyNotFoundException" >
		/// Thrown when a Property specified by the <c>propertyName</c> could not
		/// be found in the <see cref="System.Type"/>.
		/// </exception>
		public IGetter GetGetter( System.Type theClass, string propertyName )
		{
			BasicGetter result = BasicPropertyAccessor.GetGetterOrNull( theClass, propertyName );
			if( result == null )
			{
				throw new PropertyNotFoundException( "Could not find a getter for property " + propertyName + " in class " + theClass.FullName );
			}
			return result;
		}

		/// <summary>
		/// Create a <see cref="FieldSetter"/> to <c>set</c> the value of the Property
		/// through a <c>field</c>.
		/// </summary>
		/// <param name="theClass">The <see cref="System.Type"/> to find the Property in.</param>
		/// <param name="propertyName">The name of the Property to set.</param>
		/// <returns>
		/// The <see cref="FieldSetter"/> to use to set the value of the Property on an
		/// instance of the <see cref="System.Type"/>.
		/// </returns>
		/// <exception cref="PropertyNotFoundException" >
		/// Thrown when a field for the Property specified by the <c>propertyName</c> using the
		/// <see cref="IFieldNamingStrategy"/> could not be found in the <see cref="System.Type"/>.
		/// </exception>
		public ISetter GetSetter( System.Type theClass, string propertyName )
		{
			string fieldName = namingStrategy.GetFieldName( propertyName );
			return new FieldSetter( FieldAccessor.GetField( theClass, fieldName ), theClass, fieldName );
		}

		#endregion
	}
}