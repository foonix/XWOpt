namespace SchmooTech.XWOpt.OptNode
{
    public class NameNode : NodeCollection
    {
        /*
         * Node with single child and long name.
         *
         * Example in SHUTTLE.OPT
         * A2A: Pointer to Lambda_Fuse_Roof  (start of this block)
		 *	0
		 *	1
		 *	Jump to A53 (which is right after Lambda_Fuse_Roof null terminator)
		 *	1
		 *	FFFC E77F (reverse relative jump)
		 *	"Lambda_Fuse_Roof"
		 *	A53: pointer to A57
		 *	A57: (beginning of child block which is a standard NodeCollection)
         */

        public string Name { get; set; }

        internal NameNode(OptReader reader, int nameOffset) : base()
        {
            var child_pp = reader.ReadInt32();

            reader.Seek(nameOffset);
            // arbitrary length null-terminated string
            Name = reader.ReadString(maxLen: 32);

            ReadChildren(reader, 1, child_pp);
        }
    }
}
