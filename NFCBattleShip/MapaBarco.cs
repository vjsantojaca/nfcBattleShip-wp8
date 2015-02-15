using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCBattleShip
{
    public class MapaBarco
    {
        int[][] mapaBarcos = new int[10][];
        int turno;

        public MapaBarco(int[][] mapaBarcos, int turno)
        {
            this.mapaBarcos = mapaBarcos;
            this.turno = turno;
        }

        public int Turno
        {
            get { return turno; }
            set { turno = value; }
        }

        public int[][] MapaBarcos
        {
            get { return mapaBarcos; }
        }
    }
}
