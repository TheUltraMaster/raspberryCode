using Firebase.Database;

using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Device.Gpio;
using Firebase.Database.Query;

class Program
{
    private static FirebaseClient firebase;
    private static GpioController gpioController;
    private const int ledPin = 18; // Pin GPIO que controlará el LED
    
    private const int PinSensor = 21;// Pin entrada GPIO que controlará el LED
    //public  FirebaseClient firebaseClient = new FirebaseClient(FirebaseUrl);
    public static void Main(string[] args)
    {
         bool ledState = false;
        // Inicializar FirebaseClient con la URL de tu base de datos
        firebase = new FirebaseClient("https://raspberry-9334f-default-rtdb.firebaseio.com/");

        // Inicializar el controlador GPIO
        gpioController = new GpioController();
        gpioController.OpenPin(ledPin, PinMode.Output); // Configurar el pin como salida
        gpioController.OpenPin(PinSensor, PinMode.Input);

        gpioController.RegisterCallbackForPinValueChangedEvent(
        PinSensor,
        PinEventTypes.Falling | PinEventTypes.Rising,
        leerSensorAsync);



        // Configurar el listener para cambios en el nodo "data/example"
        var child = firebase
            .Child("data/example")
            .AsObservable<dynamic>() // Usamos 'dynamic' para simplificar el acceso a propiedades
            .Subscribe(d =>
            {
                if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                {
                    try
                    {
                        
                        if (d.Key == "estado")
                        {
                           
                            if (d.Object)
                            {
                                //Datos.led = true;
                                gpioController.Write(ledPin, PinValue.High); // Enciende el LED
                                Console.WriteLine("detectado");
                            }
                            else
                            { //  Datos.led = false;
                                gpioController.Write(ledPin, PinValue.Low); // Enciende el LED
                                Console.WriteLine("no detectado");
                            }
                        }
                        
                         if (d.Key == "led")
                        {
                           
                            if (d.Object)
                            {
                                Datos.led = true;
                                gpioController.Write(ledPin, PinValue.High); // Enciende el LED
                              
                            }
                            else
                            {  Datos.led = false;
                                gpioController.Write(ledPin, PinValue.Low); // Enciende el LED
                              
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine($"Error al procesar el dato: {ex.Message}");
                    }
                }
                else if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                {
                    Console.WriteLine($"Dato eliminado: {d.Key}");
                }
            });

        // Mantener la aplicación corriendo
        Console.WriteLine("Escuchando cambios en Firebase...");
        Console.ReadLine();

        // Al finalizar, liberar el pin GPIO
        gpioController.ClosePin(ledPin);
        gpioController.ClosePin(PinSensor);

    }
    //sensor
    static async void leerSensorAsync(object sender, PinValueChangedEventArgs args)
    {
        if (args.ChangeType == PinEventTypes.Rising)
        {
            
            Datos.sensor = true;
            
             await firebase
            .Child("data/example/") // Ruta do  nde deseas guardar datos
             .PutAsync(new
                {
                    led = Datos.led,
                    estado = Datos.sensor
                });

    
            

        }
        else if (args.ChangeType == PinEventTypes.Falling)  
        {
             Datos.sensor = false;
             
            
             await firebase
            .Child("data/example/") // Ruta do  nde deseas guardar datos
             .PutAsync(new
                {
                    led = Datos.led,
                    estado = Datos.sensor
                });

            
        }
        //Nueva insercion
    } 
     public class Datos
    {
        public static bool led;
        public static bool sensor;



    }
   
    


}