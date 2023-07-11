using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniBuildtest.Services
{
	public  class UserService
	{
		public string Name { get; set; } = string.Empty;
		public string Password { get; set; }= string.Empty;
		public string Email { get; set; }= string.Empty;	
		public string Phone { get; set; }=string.Empty;
		public string Subscription { get; set; }= string.Empty;	
		public string token { get; set; } = string.Empty;
		public UserService() { }

	}
}
