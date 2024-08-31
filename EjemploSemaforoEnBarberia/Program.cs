using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

class Barberia
{
    //Declaracion e Instancias
    private static int sillasDisponibles = 3; // Número de sillas de espera en la barbería.
    private static Semaphore sillasEspera = new Semaphore(sillasDisponibles, sillasDisponibles); // Semáforo para controlar las sillas de espera.
    private static Semaphore barberoDisponible = new Semaphore(0, 3); // Semáforo para indicar la disponibilidad de los barberos.
    private static Mutex mutex = new Mutex(); // Mutex para proteger la cola de clientes y la variable de trabajo de los barberos.
    private static Queue<int> colaClientes = new Queue<int>(); // Cola para almacenar los clientes que están esperando.
    private static Random Random = new Random(); //Lo utilizaremos para poder generar un numero aleatorio.

    static void Main(string[] args)
    {
        //Inicio del Programa.
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("------ La Barberia está ABIERTA --------");
        Console.ResetColor();

        // Creacion de los hilos para los tres barberos.
        Thread hiloBarberoA = new Thread(BarberoA);
        Thread hiloBarberoB = new Thread(BarberoB);
        Thread hiloBarberoC = new Thread(BarberoC);

        //Iniciamos los hilos.
        hiloBarberoA.Start();
        hiloBarberoB.Start();
        hiloBarberoC.Start();

        // Creamos hilos para los clientes que llegan aleatoriamente.
        for (int i = 1; i <= 10; i++)
        {
            Thread hiloCliente = new Thread(Cliente); //Creamos un hilo que ejecutará el metodo Cliente.
            hiloCliente.Start(i); // Es el identificador del cliente se pasa como parámetro.
            Thread.Sleep(new Random().Next(500, 1500)); // Esto simula la llegada de los clientes en intervalos aleatorios.
        }
    }

    static void BarberoA()
    {
        Barbero("A");
    }

    static void BarberoB()
    {
        Barbero("B");
    }

    static void BarberoC()
    {
        Barbero("C");
    }

    static void Barbero(string nombreBarbero)
    {
        while (true)
        {
            // Cambiar el color del texto a amarillo para los mensajes del barbero
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Barbero {nombreBarbero} está libre, esperando a un cliente.");
            Console.ResetColor();

            // Espera hasta que haya clientes en la cola.
            barberoDisponible.WaitOne();

            int clienteId;

            // Sección crítica para acceder a la cola de clientes.
            mutex.WaitOne();
            if (colaClientes.Count > 0)
            {
                // Si hay clientes en la cola, obtener el siguiente cliente que debe ser atendido.
                clienteId = colaClientes.Dequeue(); // Obtener el cliente que debe ser atendido.
            }
            else
            {
                // Si no hay clientes en la cola, libera el mutex (bloqueo) ya que no hay trabajo para hacer en este momento.
                mutex.ReleaseMutex();
                // Continua el ciclo para volver a esperar nuevos clientes.
                continue; // Volver a esperar clientes.
            }
            // Libera el mutex(bloqueo) después de procesar la cola de clientes o si no había clientes.
            mutex.ReleaseMutex();

            // Atender al cliente.
            Console.ForegroundColor= ConsoleColor.Green;
            Console.WriteLine($"Barbero {nombreBarbero} está atendiendo al cliente {clienteId}.");
            Console.ResetColor();
            Thread.Sleep(1000*Random.Next(2,5)); // Simula el tiempo de corte de cabello.

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Barbero {nombreBarbero} ha terminado de atender al cliente {clienteId}.");
            Console.ResetColor();
            
            //Salida del cliente.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"El cliente {clienteId} aprueba el corte, paga y sale de la barberia.");
            Console.ResetColor(); // Restablecer el color al predeterminado después de cada operación del barbero.
        }
    }

    static void Cliente(object idCliente)
    {
        int id = (int)idCliente;

        // Cambiar el color del texto a cian para los mensajes del cliente
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Cliente {id} ha llegado a la barbería.");

        // Intentar ocupar una silla de espera.
        if (sillasEspera.WaitOne(1000)) // Espera hasta 1 segundo para encontrar una silla.
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Cliente {id} está esperando en una silla.");

            // Sección crítica para añadir cliente a la cola.
            mutex.WaitOne();
            colaClientes.Enqueue(id); // Agregar el cliente a la cola de espera.
            barberoDisponible.Release(); // Notificar a un barbero disponible que hay un cliente esperando.
            mutex.ReleaseMutex();

            // Cliente espera a ser atendido.
            sillasEspera.Release(); // Deja la silla cuando es atendido.
        }
        else
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Cliente {id} no encontró silla y se fue.");
        }

        Console.ResetColor(); // Restablecer el color al predeterminado después de cada operación del cliente.
    }
}