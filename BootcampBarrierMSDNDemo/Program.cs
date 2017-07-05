using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BootcampBarrierMSDNDemo
{
    class Program
    {
        // Demonstrates:
        //      Barrier constructor with post-phase action
        //      Barrier.AddParticipants()
        //      Barrier.RemoveParticipant()
        //      Barrier.SignalAndWait(), incl. a BarrierPostPhaseException being thrown

        static void Main(string[] args)
        {
            int count = 0;

            // Create a barrier with 3 participants
            // Provide a post-phase action that will print out certain information
            // And the third time through, it will throw an exception.

            Barrier barrier = new Barrier(3, (b)=>
            {
                Console.WriteLine("Post phase action: count={0}, phase={1}",
                    count, b.CurrentPhaseNumber);
                if (b.CurrentPhaseNumber == 2) throw new Exception("D'oh!");
            });

            // new requirement, make it 5 participants
            barrier.AddParticipants(2);

            // nope, -- let's settle on 4 participants
            barrier.RemoveParticipant();

            // this is the logic run by all participants
            Action action = () =>
            {
                Interlocked.Increment(ref count);
                barrier.SignalAndWait(); // during the post-phase action, count should be 4
                Interlocked.Increment(ref count);
                barrier.SignalAndWait(); // during the post-phase action, count should be 8

                // the third time, SignalAndWait() will throw exception and all participants will see it
                Interlocked.Increment(ref count);
                try
                {
                    barrier.SignalAndWait();
                }
                catch (BarrierPostPhaseException bppe)
                {
                    Console.WriteLine("Caught BarrierPostPhaseException: {0}", bppe.Message);
                }

                // The fourth time should be hunky-dory
                Interlocked.Increment(ref count);
                Console.WriteLine("{0} ", count);
                barrier.SignalAndWait(); // during the post-phase action, count should be 16 and phase should be 3
            };
            
            // Now launch 4 parallel actions to serve as 4 participants
            Parallel.Invoke(action, action, action, action);

            // This (5 participants) would cause an exception
            //Parallel.Invoke(action, action, action, action, action);
            // "System.InvalidException: the number of threads using the barrier exceeded the total number of registered participants

            // it's good form to Dispose() a barrier when you're done with it.
            barrier.Dispose();
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
        }

        }
    }

