using System;

namespace NHibernate.Test.IdTest
{
	/// <summary>
	/// Summary description for StringClobClass.
	/// </summary>
	public class HiLoInt32Class
	{
		private Int32 _id;
		private string _clob;

		public Int32 Id
		{
			get { return _id; }
			set { _id = value; }
		}

	}
}

