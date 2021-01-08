using System.Collections.Generic;

namespace GameProject {
    public class Calendar {
        public List<Point> ActiveDays {
            get;
            set;
        } = new List<Point>();

        public class Point {
            public int X {
                get;
                set;
            }
            public int Y {
                get;
                set;
            }
        }
    }
}
