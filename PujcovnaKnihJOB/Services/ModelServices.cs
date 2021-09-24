using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using PujcovnaKnihJOB.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PujcovnaKnihJOB.Services
{
    class ModelServices
    {
        private dbEntities db = new dbEntities();

        public void checkBooks()
        {
            var orders = from m in db.Orders select m;
            var books = from n in db.Books select n;
            string borrowState = "Žádost o zapůjčení";
            string returnState = "Žádost o vrácení";

            foreach(var order in orders)
            {
                if (order.State.Equals(borrowState))
                {
                    Books book = db.Books.Find(order.BookID);
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State);
                    book.IsAvailable = "Půjčena";
                    order.State = "Půjčena";
                    order.BorrowDate = DateTime.Now;
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State + " Date: " + order.BorrowDate);
                    Console.WriteLine("Kniha byla zapůjčena");
                    db.Entry(book).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;    
                } else if(order.State.Equals(returnState))
                {
                    Books book = db.Books.Find(order.BookID);
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State);
                    book.IsAvailable = "Volná";
                    order.State = "Vrácena";
                    order.ReturnDate = DateTime.Now;
                    //Console.WriteLine("Titul: " + book.Title + " Status: " + book.IsAvailable + " Order state: " + order.State + " Date: " + order.ReturnDate);
                    Console.WriteLine("Kniha byla vrácena");
                    db.Entry(book).State = EntityState.Modified;
                    db.Entry(order).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
        }

        public async Task invoiceRents()
        {
            var sender = new SmtpSender(() => new SmtpClient("localhost")
            {
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = 25
            });

            StringBuilder template = new StringBuilder();
            template.AppendLine("Vážený zákazníku,");
            template.AppendLine("<p>Děkujeme za to, že jste si od nás zapůjčili knihu @Model.BookTitle.</p>");
            template.AppendLine("<p>Údaje k fakturaci jsou uvedené níže.</p>");
            template.AppendLine("<table><th>Jméno a příjmení:</th><td>@Model.FName @Model.LName</td></table>");
            template.AppendLine("<table><th>Číslo objednávky:</th><td>@Model.OrderID</td></table>");
            template.AppendLine("<table><th>Cena za den:</th><td>@Model.Price Kč</td></table>");
            template.AppendLine("<table><th>Den zapůjčení:</th><td>@Model.BorrowDate</td></table>");
            template.AppendLine("<table><th>Den vrácení:</th><td>@Model.ReturnDate</td></table>");
            template.AppendLine("<table><th>Celková cena:</th><td>@Model.FullPrice</td></table>");
            template.AppendLine("<p>S pozdravem</p>");
            template.AppendLine("<p>Vaše Půjčovna knih</p>");

            var orders = db.Orders.Where(o => o.State.Equals("Vrácena") && o.Invoiced.Equals("Ne"));
            foreach(var order in orders)
            {
                var bDay = order.BorrowDate;
                var rDay = order.ReturnDate;
                var time = ((TimeSpan)(bDay - rDay)).Days;
                decimal finalPrice;
                
                var user = db.Users.Find(order.CustomerID);
                var book = db.Books.Find(order.BookID);

                if (time == 0)
                {
                    finalPrice = book.Price;
                } else
                {
                    finalPrice = time * book.Price;
                }

                Email.DefaultSender = sender;
                Email.DefaultRenderer = new RazorRenderer();

                var email = await Email
                    .From("admin@pujcovnaknih.com")
                    .To(user.Email)
                    .Subject("Děkujeme, že jste si půjčili naši knihu!")
                    .UsingTemplate(template.ToString(), new { FName = user.FName, LName = user.LName, BookTitle = book.Title, OrderID = order.ID, Price = book.Price, 
                        BorrowDate = order.BorrowDate, ReturnDate = order.ReturnDate, FullPrice = finalPrice })
                    .SendAsync();
                order.Invoiced = "Ano";
                order.InvoiceDate = DateTime.Now;
                db.Entry(order).State = EntityState.Modified;
            }
            db.SaveChanges();
        }
    }
}
