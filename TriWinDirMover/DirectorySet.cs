namespace TriWinDirMover
{
    public class DirectorySet
    {
        public Directory Source
        {
            get;
            private set;
        }

        public Directory Target
        {
            get;
            private set;
        }

        public DirectorySet(string source, string target)
        {
            Source = new Directory(source);
            Target = new Directory(target);
        }

        public override int GetHashCode()
        {
            return Source.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return GetHashCode().Equals(obj.GetHashCode());
        }
    }
}