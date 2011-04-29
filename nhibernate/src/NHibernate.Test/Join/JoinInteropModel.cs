namespace NHibernate.Test.Join
{
    public class Info
    {
        public virtual int Id { get; set; }
        public virtual int? Version { get; set; }
        public virtual string Data { get; set; }
		
		// This property is in the joined table
		public virtual string MoreData { get; set; }
    }
}
