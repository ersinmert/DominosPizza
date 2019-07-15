using Dominos.Common.Classes;
using Dominos.Common.DTO.Input;
using Dominos.Common.DTO.Output;
using Dominos.Data.Models;
using Dominos.Data.Repository;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Dominos.Business.CustomerService
{
    public class CustomerService : ICustomerService
    {
        public CustomerService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        private readonly IRepository<Customer> _customerRepository;

        public async Task<ResponseEntity<CustomerOutputDTO>> GetCustomerById(int customerId)
        {
            var response = new ResponseEntity<CustomerOutputDTO>();
            try
            {
                var customer = await _customerRepository.FirstAsync(a => a.IsActive && a.Id == customerId);
                response.Result = new CustomerOutputDTO
                {
                    CustomerId = customer.Id,
                    Name = customer.Name,
                    Surname = customer.Surname,
                    Address = customer.Address,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber
                };
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseEntity<CustomerOutputDTO>> Login(LoginInputDTO input)
        {
            var response = new ResponseEntity<CustomerOutputDTO>();
            try
            {
                var customer = await _customerRepository.FirstAsync(a => a.IsActive && a.Email == input.Email && a.Password == input.Password);
                if (customer == null)
                {
                    response.HttpCode = HttpStatusCode.Unauthorized;
                    return response;
                }
                response.Result = new CustomerOutputDTO
                {
                    CustomerId = customer.Id,
                    Name = customer.Name,
                    Surname = customer.Surname,
                    Address = customer.Address,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber
                };
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseEntity<CustomerOutputDTO>> Register(RegisterInputDTO input)
        {
            var response = new ResponseEntity<CustomerOutputDTO>();
            try
            {
                var customer = await _customerRepository.FirstAsync(x => x.Email == input.Email);
                if (customer?.IsActive == false)
                {
                    customer.IsActive = true;
                    customer.Password = input.Password;
                    customer.Name = input.Name;
                    customer.Surname = input.Surname;
                    await _customerRepository.UpdateAsync(customer);

                    response.Result = new CustomerOutputDTO
                    {
                        CustomerId = customer.Id,
                        Name = customer.Name,
                        Surname = customer.Surname,
                        Email = customer.Email
                    };
                }
                else if (customer?.IsActive == true)
                {
                    response.Messages.Add("Bu email adresi ile daha önce kullanıcı oluşturulmuştur.");
                    response.HttpCode = HttpStatusCode.BadRequest;
                }
                else if (customer == null)
                {
                    customer = new Customer
                    {
                        Email = input.Email,
                        Name = input.Name,
                        Surname = input.Surname,
                        CreateDate = DateTime.Now,
                        IsActive = true,
                        Password = input.Password
                    };
                    await _customerRepository.InsertAsync(customer);
                    response.Result = new CustomerOutputDTO
                    {
                        CustomerId = customer.Id,
                        Name = customer.Name,
                        Surname = customer.Surname,
                        Email = customer.Email
                    };
                }
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }
    }
}