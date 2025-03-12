using System.ComponentModel.DataAnnotations;

namespace Entrvo.Models
{
  public class ApiDestinationModel
  {
    public string Server { get; set; }

    [Display(Name = "User name")]
    public string UserName { get; set; }
    public virtual string Password { get; set; }

    [Range(0, 99999)]
    [Display(Name = "Contract No.")]
    public int ContractNumber { get; set; }
    [Range(0, 99999)]
    [Display(Name = "Instance No.")]
    public int InstanceNumber { get; set; }

    [Range(0, 999)]
    [Display(Name = "Template No.")]
    public int TemplateNumber { get; set; }

    public ApiDestinationModel()
    {
      Server = @"https://209.151.135.7:8443";
    }
  }

  public class ApiDestinationEditModel : ApiDestinationModel
  {
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public override string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
  }
}
