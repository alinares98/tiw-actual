using System;
using Microsoft.SPOT;

namespace NetduinoController
{
    public class Datos
    {
        public static int num_rondas;
        public static Double[] tempMax = new Double[15]; // In ºC
        public static Double[] tempMin = new Double[15]; // In ºC
        public static Double[] tempMean = new Double[15]; // In ºC
        public static int displayRefresh = 1500; // In ms
        public static int refresh = 0;
        public static Double[] medias = new Double[15];
        public static int[] roundTime = new int[15]; // in s
        public static int totalTime = 0;

        public static bool competi = false;
        public static bool error = false;

        public static int timeLeft = 0; //In s
        public static int timeInRangeTemp = 0; //In ms.
        public static Double tempAct; // In C


        public static Double errorAct = 0;  //pasado
        public static Double errorPas = 0;  //presente
        public static Double errorFut = 0;  //futuro




    }

    /*public class Datos
    {

        
        public static double tempOptimaAnt = 0;
        public void declaration()
        {
            tempMax[0] = 30;
            tempMin[0] = 25;
            displayRefresh = 500;
            roundTime[0] = 60;
        }

        public static bool competi = false;
        public static bool error = false;

        public static int timeLeft = 0; //In s
        public static int timeInRangeTemp = 0; //In ms.
        public static Double tempAct; // In C

        //COSAS RELACIONADAS CON EL ALGORITMO PID

        public static Double kProporcional = 7;
        public static Double kIntegral = 3;
        public static Double kDiferencial = 0;

        public static Double errorAct = 0;
        public static Double errorCumu = 0;
        public static Double errorAnt = 0;
        public static Double errorGrad = 0;

        public static Double tiempoAct = 0;
        public static Double tiempoAnt = 0;


    }**/
}