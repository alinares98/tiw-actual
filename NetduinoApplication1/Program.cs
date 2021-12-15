using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using N = SecretLabs.NETMF.Hardware.Netduino;
using NetduinoController.Web;
using NETDuinoWar;
using StablePoint.Hardware.OneWire;
using StablePoint.Hardware.OneWire.Devices;
using Netduino.Foundation.Displays.MicroLiquidCrystal;


namespace NetduinoController
{
    public class Program {

        private static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

        private static int rondaAct = 0;
        private static int tiempoTotal = 0;
        public static void Main() {
            // Create a WebServer

            MyWebServer server = new MyWebServer();

            // Start the WebServer
            //server.Start();

            // Start a new thread to read the temperature
            new Thread(readTemp).Start();

            new Thread(peltier).Start();

            // Thread showing display info
            new Thread(displayTemp).Start();

            // Make sure Netduino keeps running.
            while (true) {
                Debug.Print("Netduino still running...");

                Thread.Sleep(10000);
            }

        }


        private static void peltier() {

            //Relay de frio
            var relay1 = new Netduino.Foundation.Relays.Relay(N.Pins.GPIO_PIN_D12);
            //Relay de calor
            var relay2 = new Netduino.Foundation.Relays.Relay(N.Pins.GPIO_PIN_D13);

            Debug.Print("Dentro de peltier");
            var booly = false;
            while (true) {
                //Debug.Print("ffff");
                //Debug.Print(Datos.tempAct.ToString());
                
                if (Datos.tempAct > 25) 
                {
                    relay1.IsOn = true;
                    relay2.IsOn = false;
                    Thread.Sleep(500);
                    Debug.Print("Enfrinadooooooo");
                    booly = false;
                }
                else {
                    relay1.IsOn = false;
                    relay2.IsOn = true;
                    Thread.Sleep(500);
                    Debug.Print("Calentando");
                    booly = true;
                }




            }
        }



        /// <summary>
        /// Starts one round of the competition.
        /// </summary>
        public static void startRound() {
            Datos.timeInRangeTemp = 0;

            // Start a timer thread for the round
            new Thread(timer).Start();

            // Blink the led to indicate we're in competition
            new Thread(blink).Start();

            // TODO: Do the competition stuff here
            while (Datos.competi) {


                /**
                 * 
                 * TODO: Implement devices control logic here
                 * 
                 * */



                // Print current temperature
                Debug.Print(Datos.tempAct + "");

                // Wait for the refresh rate
                Thread.Sleep(Datos.refresh);
            }

            // TODO: The round has finished => Turn off devices if needed

        }

        /// <summary>
        /// Starts a timer that will indicate when the round finish
        /// </summary>
        private static void timer() {
            Datos.competi = true;
            Datos.timeLeft = Datos.roundTime[0];
            while (Datos.timeLeft > 0) {
                Datos.timeLeft--;
                Thread.Sleep(1000);
            }
            Datos.competi = false;
        }

        /// <summary>
        /// Blinks the onboard led while we're in competition
        /// </summary>
        private static void blink() {
            while (Datos.competi) {
                led.Write(!led.Read());
                Thread.Sleep(500);
            }
            led.Write(false);
        }

        /// <summary>
        /// Refresh the temp reading the sensor
        /// </summary>
        /// 

        

        private static void readTemp() {
            //ReadTemp hecho en acorde a la pagina de StablePoint.Hardware.Onewire, encontrado en el Nuget
            //Primero se crea el bus, asignando un pin especifico para ello
            var busTemp = new Bus(N.Pins.GPIO_PIN_D4);
            //Buscamos por un dispositivo que este en la familia de DS18B20, el cual seria el que estamos usando
            var deviceTemp = busTemp.ScanForDevices(Family.Ds18B20);
            Debug.Print(deviceTemp.Length + ": DS18B20 sensor found");
            if (deviceTemp.Length > 0){
                var sensorTemp = new Ds18B20(deviceTemp[0]);
                sensorTemp.SetResolution(Ds18B20.ConversionResolution.QuarterDegree);
                while (true){
                    //Repito, usando el codigo de ejemplo de StablePoint.Hardware.OneWire
                    Debug.Print("Probando a leer el sensor...");
                    if (sensorTemp.UpdateValues() == true){
                        Debug.Print("Se ha leido el sensor");
                        //if (Datos.competi == false){
                            Debug.Print("La temperatura es: " + sensorTemp.Temperature.ToString("F2") + "ºC");
                        

                        Datos.tempAct = sensorTemp.Temperature;

                    }
                    else{
                        Debug.Print("Error al leer el sensor");
                    }
                    Thread.Sleep(1000);
                }
            }


        }

        public static void displayTemp()
        {
            var lcdProvider = new GpioLcdTransferProvider(
                Pins.GPIO_PIN_D6, // RS   
                Pins.GPIO_NONE, // RW
                Pins.GPIO_PIN_D7, // enable
                Pins.GPIO_PIN_D8, // d4
                Pins.GPIO_PIN_D9, // d5
                Pins.GPIO_PIN_D10, // d6
                Pins.GPIO_PIN_D11); // d7 


            var lcd = new Lcd(lcdProvider); //incializamos el LCD
            lcd.Begin(16, 2); // LCD de 16x2
           // lcd.Write("hola");

       

            while (true)
            {

                //Limpiamos la pantalla
                lcd.Clear();
                lcd.SetCursorPosition(0, 0); //cursor en posición
                //Escribimos en la pantalla la temperatura
                
                lcd.Write("Temp Act: " + Datos.tempAct.ToString());
                Debug.Print("Prueba 14: Display");

                Thread.Sleep(1000);

                //if (Datos.competi == false && tiempoTotal != 0)
                //{
                  //  lcd.Clear();
                  //  lcd.Write("Final: " + tiempoTotal);
                  //  Thread.Sleep(Datos.displayRefresh);
              //  }
            }
        } //Termina el displayTemp



    } // Program

} // namespace
