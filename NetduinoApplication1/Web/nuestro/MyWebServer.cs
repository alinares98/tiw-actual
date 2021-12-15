using System;
using Microsoft.SPOT;
using System.Threading;

namespace NetduinoController.Web
{
    class MyWebServer
    {
        private static readonly string pass = "pass";
        private static readonly string IP = "192.168.1.4";
        private static string msgAux = "";

        private WebServer server;

        private static bool ready = false;

        /// <summary>
        /// Instantiates a new webserver with our data
        /// </summary>
        public MyWebServer()
        {
            // Enable DHCP Server
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();

            // Assign an static IP to the Netduino
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP(IP, "255.255.255.0", "192.168.1.1");

            // Instantiate a new web server.
            server = new WebServer();

            // Add a handler for commands that are received by the server.
            server.CommandReceived += new WebServer.CommandReceivedHandler(server_CommandReceived);

            // Add the commands that the server will parse.
            // http://[server-ip]/index
            // http://[server-ip]/setparams/pass/tempMax/tempMin/displayRefresh/refresh/roundTime
            // http://[server-ip]/start/pass
            // http://[server-ip]/coolermode/pass

            server.AllowedCommands.Add(new WebCommand("index", 0)); //Pantalla Inicial
            server.AllowedCommands.Add(new WebCommand("insertnumrounds", 0)); //Introducir el número de rondas
            server.AllowedCommands.Add(new WebCommand("setrounds", 2)); //Establecer los datos de cada rango
            server.AllowedCommands.Add(new WebCommand("setparams", 6));//Inputs Introducidos--> Estado Ready
            //server.AllowedCommands.Add(new WebCommand("battle", 1));//Modo Batalla -->Estado de Batalla
            server.AllowedCommands.Add(new WebCommand("start", 1));
            server.AllowedCommands.Add(new WebCommand("coolermode", 1));
            server.AllowedCommands.Add(new WebCommand("temp", 0));
            server.AllowedCommands.Add(new WebCommand("time", 0));
            //server.AllowedCommands.Add(new WebCommand("exit", 0));

        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            server.Start();
        }

        /// <summary>
        /// Handles the CommandReceived event.
        /// </summary>
        private static void server_CommandReceived(object source, WebCommandEventArgs e)
        {

            Debug.Print("Command received: " + e.Command.CommandString);

            switch (e.Command.CommandString)
            {
                case "insertnumrounds":
                    {
                        // If there is a pending message, save it to show in the page and then resets it
                        string message = msgAux;
                        if (!msgAux.Equals(""))
                        {
                            msgAux = "";
                        }

                        // Return the index to web user.
                        e.ReturnString = writeInsertNumRounds(message);
                        break;
                    }


                case "index":
                    {
                        // If there is a pending message, save it to show in the page and then resets it
                        string message = msgAux;
                        if (!msgAux.Equals(""))
                        {
                            msgAux = "";
                        }

                        // Return the index to web user.
                        e.ReturnString = writeHTML(message);
                        break;
                    }
                case "setparams":
                    {
                        // Check the password is correct
                        if (!e.Command.Arguments[0].Equals("pass"))
                        {
                            // Return feedback to web user.
                            msgAux = "La constrase&ntilde;a es incorrecta.";
                            e.ReturnString = redirect("index");
                            break;
                        }

                        // Check we're not in competition
                        if (ready)
                        {
                            // Return feedback to web user.
                            msgAux = "No se pueden cambiar los par&aacute;metros en competici&oacute;n ni una vez preparado el sistema.";
                            e.ReturnString = redirect("index");
                            break;
                        }


                        //Datos.num_rondas = int.Parse(e.Command.Arguments[1].ToString());

                        if (!checkTemp(e.Command.Arguments[1].ToString()) || !checkTemp(e.Command.Arguments[2].ToString()) ||
                            !checkTime(e.Command.Arguments[5].ToString()) || !dataNotEmpty(e.Command.Arguments[4].ToString()) || !dataNotEmpty(e.Command.Arguments[3].ToString()))
                        {
                            // Return feedback to web user.
                            msgAux = "Los paramtros estan en formato incorrecto";
                            e.ReturnString = redirect("index");
                            break;
                        }

                        if (!checkTempRange(e.Command.Arguments[1].ToString()) || !checkTempRange(e.Command.Arguments[2].ToString()))
                        {
                            // Return feedback to web user.
                            msgAux = "El rango de temperatura m&aacute;ximo es entre 30 y 12 grados C.";
                            e.ReturnString = redirect("insertnumrounds");
                            break;
                        }
                        String auxTempMax = e.Command.Arguments[1].ToString();
                        String auxTempMin = e.Command.Arguments[2].ToString();
                        String auxRoundTime = e.Command.Arguments[5].ToString();

                        String[] TempMaximas = auxTempMax.Split(';');
                        String[] TempMinimas = auxTempMin.Split(';');
                        String[] RoundTimes = auxRoundTime.Split(';');

                        for (int i = 0; i < Datos.num_rondas; i++)
                        {

                            Debug.Print("Temperatura maxima de la ronda " + i + ": " + TempMaximas[i]);

                            Debug.Print("Temperatura minima de la ronda " + i + ": " + TempMinimas[i]);

                            Debug.Print("Tiempo de la ronda " + i + ": " + RoundTimes[i]);

                        }
                        // Change the params

                        for (int i = 0; i < Datos.num_rondas; i++)
                        {
                            Double auxtempMax = double.Parse(auxTempMax);
                            Double auxtempMin = double.Parse(auxTempMin);
                            Double auxtempMean = (auxtempMax + auxtempMin) / 2;
                            int auxroundTime = int.Parse(auxRoundTime);


                            Datos.tempMax[i] = auxtempMax;
                            Datos.tempMin[i] = auxtempMin;
                            Datos.tempMean[i] = auxtempMean;
                            Datos.roundTime[i] = auxroundTime;


                        }
                        Datos.displayRefresh = int.Parse(e.Command.Arguments[3].ToString());
                        Datos.refresh = int.Parse(e.Command.Arguments[4].ToString());
                        ready = true;

                        // Return feedback to web user.
                        msgAux = "Los par&aacute;metros se han cambiado satisfactoriamente. Todo preparado.";
                        e.ReturnString = redirect("index");
                        break;
                    }



                case "setrounds":
                    {

                        Datos.num_rondas = int.Parse(e.Command.Arguments[0].ToString());
                        Debug.Print("RONDAS ANTES DE REDIRECT: " + Datos.num_rondas);
                        e.ReturnString = redirect("index");
                        break;
                    }


                case "start":
                    {
                        // Check the password is correct
                        if (!e.Command.Arguments[0].Equals(pass))
                        {
                            // Return feedback to web user.
                            msgAux = "La constrase&ntilde;a es incorrecta.";
                            e.ReturnString = redirect("index");
                            break;
                        }

                        // Check we're not in cometition
                        if (Datos.competi)
                        {

                            msgAux = "Ya estamos en competici&oacute;n.";
                            e.ReturnString = redirect("index");
                            break;
                        }

                        // Start the round
                        new Thread(Program.startRound).Start();

                        // Wait for the round to finish
                        while (Datos.competi)
                        {
                            Thread.Sleep(1000);
                        }
                        ready = false;

                        // Return feedback to web user.
                        if (Datos.error)
                        {
                            Datos.error = false;
                            msgAux = "Se ha detenido la competici&oacute;n porque se detect&oacute; una temperatura superior a 40C.";
                            e.ReturnString = redirect("index");
                        }
                        else
                        {
                            msgAux = "El tiempo que se ha mantenido en el rango de temperatura es de " + System.Math.Round((Datos.timeInRangeTemp / 1000) * 10) / 10 + "s. " +
                               " y el tiempo que se ha mantenido fuera del rango de temperatura es de " + (Datos.totalTime - System.Math.Round((Datos.timeInRangeTemp / 1000) * 10) / 10) + "s. ";
                            e.ReturnString = redirect("index");
                        }
                        break;
                    }
                case "coolermode":
                    {
                        // Check the password is correct
                        if (!e.Command.Arguments[0].Equals(pass))
                        {
                            // Return feedback to web user.
                            msgAux = "La constrase&ntilde;a es incorrecta.";
                            e.ReturnString = redirect("index");
                            break;
                        }

                        // Check we're not in cometition or ready for it
                        if (ready)
                        {
                            // Return feedback to web user.
                            msgAux = "No se puede activar este modo en competici&oacute;n ni una vez preparado el sistema.";
                            e.ReturnString = redirect("index");
                            break;
                        }

                        // Starts the cooler mode
                        //new Thread(Program.coolerMode).Start();

                        // Return feedback to web user.
                        msgAux = "Se ha iniciado el modo enfriamiento satisfactoriamente.";
                        e.ReturnString = redirect("index");
                        break;
                    }
                case "temp":
                    {
                        // Return feedback to web user.
                        msgAux = "La temperatura del sistema es de " + Datos.tempAct + "C.";
                        e.ReturnString = redirect("index");
                        break;
                    }
                case "time":
                    {
                        // Return feedback to web user.
                        msgAux = "El tiempo que se ha mantenido en el rango de temperatura es de " + System.Math.Round((Datos.timeInRangeTemp / 1000) * 10) / 10 + "s. " +
                            " y el tiempo que se ha mantenido fuera del rango de temperatura es de " + (Datos.totalTime - System.Math.Round((Datos.timeInRangeTemp / 1000) * 10) / 10) + "s. ";
                        e.ReturnString = redirect("index");
                        break;
                    }
            }
        }

        private static bool dataNotEmpty(String param)
        {
            bool valid = true;
            if (param.Length == 0)
            {
                valid = false;
            }

            return valid;
        }


        private static bool checkTime(String param)
        {
            bool valid = true;
            String[] time = param.Split(';');
            foreach (string elem in time)
            {
                try
                {
                    int isNumeric = int.Parse(elem);
                }
                catch (Exception e)
                {
                    valid = false;
                    Debug.Print("Incorrect format.");
                }
            }

            return valid;
        }

        private static bool checkTemp(String param)
        {
            bool valid = true;
            String[] temperature = param.Split(';');
            foreach (string elem in temperature)
            {
                try
                {
                    double isNumeric = double.Parse(elem);

                }
                catch (Exception e)
                {
                    valid = false;
                    Debug.Print(" Incorrect format.");
                }
            }

            if (temperature.Length != Datos.num_rondas)
            {
                valid = false;
                Debug.Print("Numero de rondas erronea");
            }
            return valid;
        }

        private static bool checkTempRange(String param)
        {
            bool valid = true;
            String[] temperature = param.Split(';');
            foreach (string elem in temperature)
            {
                double temp = double.Parse(elem);
                if (temp > 30 || temp < 12)
                {
                    valid = false;
                }
            }

            if (temperature.Length != Datos.num_rondas)
            {
                valid = false;
                Debug.Print("Rango de temperaturas erroneo");
            }
            return valid;
        }

        private static bool checkRondas(String param)
        {
            bool valid = true;
            String[] rondas = param.Split(';');
            foreach (string elem in rondas)
            {
                try
                {
                    int isNumeric = int.Parse(elem);
                }
                catch (Exception e)
                {
                    valid = false;
                    Debug.Print(" Incorrect format.");
                }
            }

            return valid;
        }

        /// <summary>
        /// Create an HTML webpage.
        /// </summary>
        /// <param name="message">The message you want to be shown.</param>
        /// <returns>String with the HTML page desired.</returns>
        public static string writeInsertNumRounds(String message)
        {
            // If we are already ready, disable all the inputs
            string disabled = "";
            if (ready) disabled = "disabled";

            string rondas = "<a href='#' onclick='rondas()'> Insertar Rondas </a>";

            //Write the HTML page
            string html = "<!DOCTYPE html><html><head><title>Netduino Plus 2 Controller</title>" +
                            "<script>function rondas(){" +
                            "var numrondas = document.forms['params']['numrondas'].value;" +
                            "window.location = 'http://" + IP + "/setrounds/' +  numrondas;}" +
                            "function sacarRondas(){window.location = 'http://" + IP + "/index/' +  numrondas;}" +
                            "</script><body><p style='padding:10px;font-weight:bold;'>" + message + "</p><form name='params'>" +
                            "<p>Numero de rondas <b>(&deg;C)</b> <input name='numrondas' type='number' value='" + Datos.num_rondas + "'></input></p>" +
                            "</form> " + rondas + " <br/>";

             
            //"</form> "+ rondas +" <br/><a href='#' onclick='sacarRondas()'> Aceptar </a></body>"
            return html;
        }

        /// <summary>
        /// Create an HTML webpage.
        /// </summary>
        /// <param name="message">The message you want to be shown.</param>
        /// <returns>String with the HTML page desired.</returns>
        public static string writeHTML(String message)
        {
            // If we are already ready, disable all the inputs
            string disabled = "";
            if (ready) disabled = "disabled";

            // Only show save and cooler mode in configuration mode and start round when we are ready
            string save = "<a href='#' onclick='rondas()'>Estoy Preparado - Guardar</a>";
            if (ready) save = "";
            string start = "";
            if (ready) start = "<a style='padding-left:80px;' href='#' onclick='start()'>Comenzar Ronda</a>";
            if (Datos.competi) start = "";
            string cooler = "<a href='#' onclick='coolerMode()'>Modo Enfriamiento</a>";
            if (ready) cooler = "";

            string inputs = "";
            string htmlfinal = "<!DOCTYPE html><html><head><title>Netduino Plus 2 Controller</title>" +

                            "<script>" +
                            "function rondas(){" +

                            "  var form = document.forms.params;" +

                            "  var tempMax = [];" +

                            " var tempMin = [];" +

                            " var time = [];" +

                            " var displayRefresh = form[form.length-3].value;" +

                            " var refresh = form[form.length-2].value;" +

                            " var pass = form[form.length-1].value;" +

                            "for(let i = 0;" +
                            " i < form.length-3;" +
                            " i+=3) " +
                            "{" +
                            "    tempMax.push(form[i].value);" +

                            "   tempMin.push(form[i+1].value);" +

                            "  time.push(form[i+2].value);" +

                            "}" +
                            "console.log(tempMax);" +

                            "console.log(tempMin);" +

                            "console.log(time);" +

                            "console.log(displayRefresh.value);" +

                            "console.log(refresh.value);" +

                            "console.log(pass.value);" +

                            "window.location = 'http://" + IP + "/setparams/' + pass + '/' + tempMax + '/' + tempMin + '/' + displayRefresh + '/' + refresh + '/' + time;}" +

                            "function start(){var pass = document.forms['params']['pass'].value;window.location = 'http://" + IP + "/start/' + pass;}" +
                            "function time(){window.location = 'http://" + IP + "/time';}" +
                            "function temp(){window.location = 'http://" + IP + "/temp';}" +
                            "function coolerMode(){var pass = document.forms['params']['pass'].value;window.location = 'http://" + IP + "/coolermode/' + pass;}" +
                            "</script>" +

                            "</head>" +
                            "<body><p style='padding:10px;font-weight:bold;'>" + message + "</p><form name='params'></body>";


            for (int i = 0; i < Datos.num_rondas; i++)
            {
                inputs += "<p>Temperatura Max en la ronda " + i + " <b>(&deg;C)</b> <input name='tempMax " + i + " ' type='number' max='30' min='12' step='0.1' value='" + Datos.tempMax + "' " + disabled + "></input></p>" +
                "<p>Temperatura Min en la ronda" + i + "<b>(&deg;C)</b> <input name='tempMin " + i + "' type='number' max='30' min='12' step='0.1' value='" + Datos.tempMin + "' " + disabled + "></input></p>" +
                "<p>Duraci&oacute;n Ronda <b>(s)</b> <input name='time " + i + "' type='number' value='" + Datos.roundTime + "' " + disabled + "></input></p>";
            }


            string htmlend = htmlfinal + inputs + "<p>Cadencia Refresco <b>(ms)</b> <input name='displayRefresh' type='number' value='" + Datos.displayRefresh + "' " + disabled + "></input></p>" +
                "<p>Cadencia Interna <b>(ms)</b> <input name='refresh' type='number' value='" + Datos.refresh + "' " + disabled + "></input></p>" +
                "<p>Contrase&ntilde;a <input name='pass' type='password'></input></p>" +
                "</form>" + save + start + "<br/>" + cooler + "<br/><a href='#' onclick='temp()'>Consultar Temperatura</a><br/><a href='#' onclick='time()'>Consultar Tiempo</a></body>";
            return htmlend;
        }

        /// <summary>
        /// Create an HTML page that redirect to the desired page. (used to prevent params like password showing in the url)
        /// </summary>
        /// <param name="page">The page you want to redirect.</param>
        /// <returns>String with the HTML page desired.</returns>
        public static string redirect(string page)
        {
            return "<!DOCTYPE html><html><head><meta http-equiv='refresh' content='0; url=http://" + IP + "/" + page + "'></head></html>";
        }

    }
}