using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Entrvo.Models
{
  public class FtpSourceModel: IRemoteCredential
  {
    public string Server { get; set; }

    [Display(Name = "User name")]
    public string UserName { get; set; }
    public virtual string Password { get; set; }

    [DisplayName("Root Directory")]
    public string RootDirectory { get; set; }
    public long? AverageFullFileSize { get; set; }
    public long? AverageIncrementalFileSize { get; set; }

    public FtpSourceModel()
    {
      Server = "sftpflex.t2hosted.ca";
    }
  }

  public class FtpSourceEditModel : FtpSourceModel
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

  public class FtpOptionsModel
  {
    [DisplayName("Root Directory")]
    public string RootDirectory { get; set; }

    public string[] Folders { get; set; }
  }

}