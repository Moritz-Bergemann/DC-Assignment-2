using System.Collections.Generic;
using APIClasses;

namespace ClientApplication
{
    class Blockchain
    {
        public static Blockchain Instance
        {
            get;
        } = new Blockchain();

        public Block LastBlock { get; private set; }

        public List<Block> Chain
        {
            get => _chain;
            set
            {
                _chain = value;
                LastBlock = value[value.Count - 1];
            }
        }

        private List<Block> _chain;

        private Blockchain()
        {
            _chain = null;
            LastBlock = null;
        }
    }
}
