using Microsoft.AspNetCore.Identity;

namespace LogiTrack.Model
{
  public class ApplicationUser : IdentityUser
  {
    public string? FullName { get; set; }
  }
}