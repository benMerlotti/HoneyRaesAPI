using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
using Microsoft.AspNetCore.Authentication;

List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Alice Johnson", Address = "123 Maple Street" },
    new Customer { Id = 2, Name = "Bob Smith", Address = "456 Oak Avenue" },
    new Customer { Id = 3, Name = "Carol Williams", Address = "789 Pine Road" }
};
List<Employee> employees = new List<Employee>
{
    new Employee { Id = 1, Name = "John Doe", Specialty = "Software Development" },
    new Employee { Id = 2, Name = "Jane Smith", Specialty = "Graphic Design" }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Fix software bug in payment system",
        Emergency = true,
        DateCompleted = DateTime.Now.AddDays(-5)
    },
    new ServiceTicket
    {
        Id = 2,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "Design new marketing materials",
        Emergency = false,
        DateCompleted = DateTime.Now.AddDays(-10)
    },
    new ServiceTicket
    {
        Id = 3,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "Implement new feature in client portal",
        Emergency = true,
        DateCompleted = DateTime.Now.AddDays(-2)
    },
    new ServiceTicket
    {
        Id = 4,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Redesign company website layout",
        Emergency = false,
        DateCompleted = DateTime.Now.AddDays(-8)
    },
    new ServiceTicket
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Update security protocols on server",
        Emergency = true,
        DateCompleted = DateTime.Now.AddDays(-1)
    }
};



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    Employee employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{

    // Get the customer data to check that the customerid for the service ticket is valid
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    // if the client did not provide a valid customer id, this is a bad request
    if (customer == null)
    {
        return Results.BadRequest();
    }

    // creates a new id (SQL will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });

});

app.MapDelete("servicetickets/{id}", (int id) =>
{
    var serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    serviceTickets.Remove(serviceTicket);

    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/employees", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapGet("/customers", () =>
{
    return employees.Select(a => new CustomerDTO
    {
        Id = a.Id,
        Name = a.Name,
        Address = a.Specialty
    });
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new EmployeeDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Specialty = customer.Address
    });
});

app.Run();