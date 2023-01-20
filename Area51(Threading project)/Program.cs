using Area51;
using System.Collections.Generic;
using System.Threading;
using System;
using static Area51.Agent;
using System.Linq;

class Program
{
    public static Elevator elevator = new Elevator();
    public static List<Thread> agentThreads = new List<Thread>();
    static void Main()
    {
        Random random = new Random();
        List<Agent> agents = new List<Agent>();
        var securityLevels = Enum.GetValues(typeof(SecurityLevel)).Cast<SecurityLevel>().ToList();
        for (int i = 0; i < 5; i++)
        {

            agents.Add(new Agent(i + 1,
                                  securityLevels[random.Next(securityLevels.Count)]
            ));

            Thread agentThread = new Thread(agents[i].DoWork);
            agentThread.Start();
            agentThreads.Add(agentThread);
        }

        Thread elevatorThread = new Thread(elevator.Activate);
        elevatorThread.Start();
    }
}