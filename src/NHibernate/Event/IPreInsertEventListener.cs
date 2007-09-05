namespace NHibernate.Event
{
	/// <summary>
	/// Called before inserting an item in the datastore
	/// </summary>
	public interface IPreInsertEventListener
	{
		/// <summary> Return true if the operation should be vetoed</summary>
		/// <param name="thEvent"></param>
		bool OnPreInsert(PreInsertEvent thEvent);
	}
}