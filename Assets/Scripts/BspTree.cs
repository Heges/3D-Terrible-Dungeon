using UnityEngine;

namespace TerribleDungeon
{
    public class BspTree
    {
        public RectInt container;
        public RectInt room;
        public BspTree left;
        public BspTree right;
        public int bspTreeId;

        public  static int MIN_ROOM_SIZE = 4;

        public static int debugId;

        public int currentId;

        private const int MIN_ROOM = 5;

        public BspTree(RectInt a)
        {
            container = a;
            currentId = debugId++;
        }

        internal static BspTree Split(int numberOfOperations, RectInt container)
        {
            var node = new BspTree(container);

            if (numberOfOperations == 0)
            {
                return node;
            }

            var splitedContainer = SplitContainer(container);

            node.left = Split(numberOfOperations - 1, splitedContainer[0]);

            node.right = Split(numberOfOperations - 1, splitedContainer[1]);

            return node;
        }

        private static RectInt[] SplitContainer(RectInt container)
        {
            RectInt c1, c2;
            if (container.width < MIN_ROOM)
            {
                c1 = new RectInt(0, 0, 0, 0);
                c2 = new RectInt(0, 0, 0, 0);

                return new RectInt[] { c1, c2 };
            }
            if (container.height < MIN_ROOM)
            {
                c1 = new RectInt(0, 0, 0, 0);
                c2 = new RectInt(0, 0, 0, 0);

                return new RectInt[] { c1, c2 };
            }

            bool horizontal;
            if (container.width / container.height >= 1.25f)
            {
                horizontal = true;
            }
            else if (container.height / container.width >= 1.25f)
            {
                horizontal = false;
            }
            else
            {
                horizontal = Random.Range(0f, 1f) > 0.5f ? true : false;
            }

            if (horizontal)
            {
                c1 = new RectInt(container.x, container.y, (int)Random.Range(container.width * 0.3f, container.width * 0.5f), container.height);
                c2 = new RectInt(container.x + c1.width, container.y, container.width - c1.width, container.height);
            }
            else
            {
                c1 = new RectInt(container.x, container.y, container.width, (int)Random.Range(container.height * 0.3f, container.height * 0.5f));
                c2 = new RectInt(container.x, container.y + c1.height, container.width, container.height - c1.height);
            }
            return new RectInt[] { c1, c2 };
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public bool IsInternal()
        { // de morgan's
            return left != null || right != null;
        }

        public static void GenerateRoomInsideContainersNode(BspTree node)
        {
            if (node.left == null && node.right == null)
            {
                //Debug.Log("moy current id " + node.currentId + " moy debugId " + BspTree.debugId);
                var randomX = Random.Range(MIN_ROOM_SIZE, node.container.width / 4);
                var randomY = Random.Range(MIN_ROOM_SIZE, node.container.height / 4);
                var x = node.container.x + randomX;
                var y = node.container.y + randomY;
                var widthRoom = node.container.width - randomX;// - (int)(randomX * Random.Range(1f, 1.5f));
                                                               //var widthRoom = node.container.width  - (int)(randomX * Random.Range(0.5f, 1.1f));
                var heightRoom = node.container.height - randomY;// - (int)(randomY * Random.Range(1f, 1.5f));
                                                                 //var heightRoom = node.container.height - (int)(randomY * Random.Range(0.5f, 1.1f));

                //if (widthRoom > node.container.width)
                //{
                //    widthRoom = (int)(widthRoom * Random.Range(0.3f, 0.6f));
                //}
                //if (heightRoom > node.container.width)
                //{
                //    heightRoom = (int)(heightRoom * Random.Range(0.3f, 0.6f));
                //}
                //if (widthRoom < MIN_ROOM_SIZE)
                //{
                //    widthRoom = MIN_ROOM_SIZE;
                //}
                //if (heightRoom < MIN_ROOM_SIZE)
                //{
                //    heightRoom = MIN_ROOM_SIZE;
                //}
                node.room = new RectInt(x, y, widthRoom, heightRoom);
            }
            else
            {
                if (node.left != null)
                {
                    GenerateRoomInsideContainersNode(node.left);
                }
                if (node.right != null)
                {
                    GenerateRoomInsideContainersNode(node.right);
                }
            }
        }
    }
}
     
