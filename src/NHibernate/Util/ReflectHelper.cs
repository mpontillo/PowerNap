using System;
using System.Reflection;
using NHibernate.Type;

namespace NHibernate.Util {
	
	public sealed class ReflectHelper {
		private static System.Type[] NoClasses = new System.Type[0];
		private static System.Type[] Object = new System.Type[] { typeof(object) };
		private static MethodInfo ObjectEquals;

		public sealed class Setter {
			private System.Type clazz;
			private PropertyInfo property;
			private string propertyName;

			public Setter(System.Type clazz, PropertyInfo property, string propertyName) {
				this.clazz = clazz;
				this.property = property;
				this.propertyName = propertyName;
			}

			public void Set(object target, object value) {
				try {
					property.SetValue(target, value, new object[0]);
				} catch (Exception e) {
					throw new PropertyAccessException(e, "Exception occurrded", true, clazz, propertyName);
				}
			}
			public PropertyInfo Property {
				get { return property; }
			}
		}

		public sealed class Getter {
			private System.Type clazz;
			private PropertyInfo property;
			private string propertyName;

			public Getter(System.Type clazz, PropertyInfo property, string propertyName) {
				this.clazz = clazz;
				this.property = property;
				this.propertyName = propertyName;
			}

			public object Get(object target) {
				try {
					return property.GetValue(target, new object[0]);
				} catch (Exception e) {
					throw new PropertyAccessException(e, "Exception occurrded", false, clazz, propertyName);
				}
			}
			public System.Type ReturnType {
				get { return property.PropertyType; }
			}
			public PropertyInfo Property {
				get { return property; }
			}
		}

		static ReflectHelper() {
			MethodInfo eq;
			try {
				eq = typeof(object).GetMethod("Equals");
			} catch (Exception e) {
				throw new AssertionFailure("Could not find Object.Equals()", e);
			}
			ObjectEquals = eq;
		}

		public static bool OverridesEquals(System.Type clazz) {
			MethodInfo equals;
			try {
				equals = clazz.GetMethod("Equals");
			} catch (Exception) {
				return false;
			}
			return !ObjectEquals.Equals(equals);
		}

		public static Setter GetSetter(System.Type type, string propertyName) {
			Setter result = GetSetterOrNull(type, propertyName);
			if (result==null) throw new PropertyNotFoundException( "Could not find a setter for property " + propertyName + " in class " + type.FullName );
			return result;
		}

		private static Setter GetSetterOrNull(System.Type type, string propertyName) {
			if (type == typeof(object) || type == null) return null;

			PropertyInfo property = type.GetProperty(propertyName);

			if (property != null) {
				return new Setter(type, property, propertyName);
			} else {
				Setter setter = GetSetterOrNull( type.BaseType, propertyName );
				if (setter == null) {
					System.Type[] interfaces = type.GetInterfaces();
					for ( int i=0; setter==null && i<interfaces.Length; i++) {
						setter = GetSetterOrNull(interfaces[i], propertyName);
					}
				}
				return setter;
			}
		}

		public static Getter GetGetter(System.Type theClass, string propertyName) {
			Getter result = GetGetterOrNull(theClass, propertyName);
			if (result == null) throw new PropertyNotFoundException( "Could not find a setter for property " + propertyName + " in class " + theClass.FullName );
			return result;
		}

		private static Getter GetGetterOrNull(System.Type type, string propertyName) {
			if (type==typeof(object) || type==null) return null;

			PropertyInfo property = type.GetProperty(propertyName);

			if (property != null) {
				return new Getter(type, property, propertyName);
			} else {
				Getter getter = GetGetterOrNull( type.BaseType, propertyName );
				if (getter == null) {
					System.Type[] interfaces = type.GetInterfaces();
					for (int i=0; getter==null && i<interfaces.Length; i++) {
						getter = GetGetterOrNull( interfaces[i], propertyName );
					}
				}
				return getter;
			}

		}

		public static IType ReflectedPropertyType(System.Type theClass, string name) {
			return TypeFactory.HueristicType( GetGetter(theClass, name).ReturnType.Name );
		}

		public static System.Type ClassForName(string name) {
			return System.Type.GetType(name);
		}

		public static object GetConstantValue(string name) {
			System.Type clazz;
			try {
				clazz = ClassForName( StringHelper.Qualifier(name) );
			} catch(Exception) {
				return null;
			}
			try {
				return clazz.GetProperty( StringHelper.Unqualify(name) ).GetValue(null, new object[0]);
				//??
			} catch (Exception) {
				return null;
			}
		}

		public static ConstructorInfo GetDefaultConstructor(System.Type type) {
			if (IsAbstractClass(type)) return null;
			
			try {
				ConstructorInfo contructor = type.GetConstructor(NoClasses);
				return contructor;
			} catch (Exception) {
				throw new PropertyNotFoundException(
					"Object class " + type.FullName + " must declare a default (no-arg) constructor"
					);
			}
		}

		public static bool IsAbstractClass(System.Type type) {
			return (type.IsAbstract || type.IsInterface);
		}

	}
}
