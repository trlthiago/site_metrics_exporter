using System;
using System.Collections.Generic;
using tester_core;
using tester_core.Models;

namespace tester_console
{
    public class Program
    {
        static Services services;
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            AppDomain.CurrentDomain.ProcessExit += StaticClass_Dtor;

            services = new Services();

            //for (int i = 0; i < 50; i++)
            //{
            //    var text = await services.AccessPage("https://blog.umbler.com/br");
            //    Console.WriteLine(text);
            //}

            foreach (var domain in Domains)
            {
                //var text = await services.AccessPage(domain.NormalizeUrl());
                var text = await services.AccessPageAndGetResourcesFull(domain.NormalizeUrl(), true);
                Console.WriteLine($"{domain,50} | {text.SiteStatus}");
            }

            Console.WriteLine("FIM");
            Console.ReadKey();
        }

        static void StaticClass_Dtor(object sender, EventArgs e)
        {
            if (services != null)
                services.Dispose();
        }

        static List<string> Domains = new List<string>
        {
            "blog.umbler.com",
"detab.ga",
            "provaonline.com.br",
"provaonlineexame.com.br",
"educandoomundo.tk",
"php.educandoomundo.tk",
"node02.detab.ga",
"php02.detag.ga",
"tatooine01.com",
"trlphpcontainer01.com.br",
"pwnedpass5.com",
"static01.trlthiago.com",
"static03.trlthiago.com",
"static04.trlthiago.com",
"static05.trlthiago.com",
"static06.trlthiago.com",
"okrq2.trlthiago.com",
"node1000.trlthiago.com",
"ubapcweb02.trlthiago.com",
"okrq7.trlthiago.com",
"laravel.trlthiago.com",
"beta01.trlthiago.com",
"roi.com",
"node03.trlthiago.com",
"lavarel.trlthiago.com",
"okrq8.trlthiago.com",
"beta02.trlthiago.com",
"okr9.trlthiago.com",
"laravel01.trlthiago.com",
"okrq10.trlthiago.com",
"shared01.trlthiago.com",
"comp01.trlthiago.com",
"phpus.trlthiago.com",
"phpc05.trlthiago.com.br",
"phpc07.trlthiago.com",
"phpc08.trlthiago.com",
"phpc03.trlthiago.com",
"phpc12.trlthiago.com",
"sqlspo1.com",
"phpc13.trlthiago.com",
"phpc14.trlthiago.com",
"nodec01.trlthiago.com",
"apc24.trlthiago.com",
"apc29.trlthiago.com",
"sartori.trlthiago.com",
"apcssd.trlthiago.com",
"apctest01.trlthiago.com",
"node04.trlthiago.com",
"static10.trlthiago.com",
"apcperfil.trlthiago.com",
"trlthiago.tk",
"apc.trlthiago.tk",
"cloud.trlthiago.tk",
"cloud02.trlthiago.tk",
"cloud03.trlthiago.tk",
"cloud04.trlthiago.tk",
"apc01.trlthiago.tk",
"justoehfoda.trlthiago.tk",
"dns01.trlthiago.tk",
"phpc15.trlthiago.tk",
"testenovomenu.com",
"node02.trlthiago.com",
"phpc16.trlthiago.tk",
"perfectoficina.tk",
"meudeuscomotarapido.com.br",
"asdasd123asd.com",
"aspnetcore30.com"
        };
    }
}
