using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase; 
using Firebase.Auth;
using System.Threading.Tasks;
using System.Linq;

public class Firebase_Manager : MonoBehaviour
{
    [Header("For UI")]
    public GameObject MainmenuUI;
    public GameObject LoginUI;
    public GameObject RegisterUI;

    [Header("For Login")]
    public TMP_InputField Email;
    public TMP_InputField Password;
    public Toggle PasswordToggle;
    public Text PasswordToggleText;

    [Header("For Register")]
    public Canvas CanvasBackgroundRegister;
    public TMP_InputField EmailRegister;
    public TMP_InputField UsernameRegister;
    public TMP_InputField PasswordRegister;
    public TMP_InputField ConfirmPass;
    public Toggle PasswordRegisterToggle;
    public Text PasswordRegisterToggleText;

    [Header("For User Menu")]
    public TMP_Text User;
    public TMP_InputField Start1;
    public TMP_InputField Start2;
    public TMP_InputField Start3;

    [Header("Untuk Firebase")] 
    public DependencyStatus depStatus;
    public FirebaseUser FbUser;
    public FirebaseAuth FbAuth;

    private void Awake()
    {
        PasswordToggleText=PasswordToggle.GetComponentInChildren<Text>();
        PasswordRegisterToggleText = PasswordRegisterToggle.GetComponentInChildren<Text>();
        StartCoroutine(CheckFirebase());
    }
    void Start()
    {
        MainmenuUI.SetActive(true);
        LoginUI.SetActive(false);
        RegisterUI.SetActive(false);

        PasswordToggle.isOn = false;
        PasswordToggleText.text = "Show Password";
        Password.contentType = TMP_InputField.ContentType.Password;

        PasswordRegisterToggle.isOn = false;
        PasswordRegisterToggleText.text = "Show Password";
        PasswordRegister.contentType = TMP_InputField.ContentType.Password;

        ConfirmPass.contentType = TMP_InputField.ContentType.Password;
    }

    private IEnumerator CheckFirebase()
    {
        Task<DependencyStatus> depTask = FirebaseApp.CheckAndFixDependenciesAsync(); // untuk mengecek apakah firebase online
        yield return new WaitUntil(() => depTask.IsCompleted); // menunggu sampai task dependency selesai

        depStatus = depTask.Result;

        if (depStatus == DependencyStatus.Available) // kalau firebase online
        {
            Debug.Log("Firebase online dan bisa digunakan");
            InitializeFirebase();
            yield return new WaitForEndOfFrame();

            StartCoroutine(AutoLoginCheck());
        }

        else
        {
            Debug.Log("Firebase offline, turu");
        }
    }

    public void InitializeFirebase()
    {
        FbAuth = FirebaseAuth.DefaultInstance;
        FbAuth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    public void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (FbAuth.CurrentUser != FbUser)
        {
            bool signedin = FbUser != FbAuth.CurrentUser && FbAuth.CurrentUser != null;

            if (!signedin && FbUser != null)
            {
                Debug.Log("Signed out " + FbUser.UserId);
            }

            FbUser = FbAuth.CurrentUser;

            if (signedin)
            {
                Debug.Log("Signed in " + FbUser.UserId);
            }
        }
    }

    private IEnumerator AutoLoginCheck()
    {
        if (FbUser != null)
        {
            Task reloadUserTask = FbUser.ReloadAsync(); // untuk memastikan usernya ada
            yield return new WaitUntil(() => reloadUserTask.IsCompleted); // membaca ulang data user dari console firebase

            AutoLogin();
        }
    }

    public void AutoLogin()
    {
        if (FbUser != null) // ekstra pengecekan untuk lebih memastikan user
        {
            Debug.Log("Auto Login Success!!");
            StartCoroutine(AutoLoginTransition());
        }

        else
        {
            MainmenuUI.SetActive(false);
            LoginUI.SetActive(true);
            //dibawa ke loginUI jika autologin gagal
        }
    }

    private IEnumerator AutoLoginTransition()
    {
        yield return new WaitForSeconds(0.8f);

        string u = PlayerPrefs.GetString("myUsername");
        User.text = u; // mengambil string dari player preferences
        Debug.Log(User.text);

        MainmenuUI.SetActive(false);
        LoginUI.SetActive(true);
        //dibawa ke sudah login
    }

    public void RegisterButton()
    {
        StartCoroutine(RegisterFirebase(EmailRegister.text, UsernameRegister.text, PasswordRegister.text, ConfirmPass.text));
    }

    private IEnumerator RegisterFirebase(string email, string username, string password, string confirmPass)
    {
        if (string.IsNullOrEmpty(email)) // cek apakah input teks kosong, true kalau kosong
        {
            Debug.Log("emailnya kosong bos");
        }

        else if (string.IsNullOrEmpty(username)) // cek apakah input teks kosong, true kalau kosong
        {
            Debug.Log("username kosong bos");
        }

        else if (password != confirmPass) // cek apakah konfirmasi password sudah cocok
        {
            Debug.Log("pastikan password cocok");
        }

        else
        {
            Task<AuthResult> registerTask = FbAuth.CreateUserWithEmailAndPasswordAsync(email, password); // daftar menggunakan email, password
            yield return new WaitUntil(() => registerTask.IsCompleted); // tunggu register sampai selesai

            if (registerTask.Exception != null) // kalau register task ada error
            {
                Debug.Log(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException; // mendeklarasi errornya apa
                AuthError authError = (AuthError)firebaseException.ErrorCode; // mengambil error code

                string failMessage = "Registration failed! Because ";

                switch (authError) // switch case itu mirip dgn if else atau if else if
                {
                    case AuthError.InvalidEmail:
                        failMessage += "Email is Invalid";
                        break;
                    case AuthError.WrongPassword:
                        failMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failMessage += "Email is missing, please provide email";
                        break;
                    case AuthError.MissingPassword:
                        failMessage += "Password is missing, please provide password";
                        break;
                    default:
                        failMessage = "Registration failed :(";
                        break;
                }
                Debug.Log(failMessage);
            }

            else // kalau register task tidak ada error
            {
                FbUser = registerTask.Result.User; // mengambil user dari hasil register task
                UserProfile uProfile = new UserProfile { DisplayName = username };

                Task ProfileTask = FbUser.UpdateUserProfileAsync(uProfile); // update user profile
                yield return new WaitUntil(() => ProfileTask.IsCompleted); // tunggu sampai update profile selesai

                Debug.Log(FbUser.DisplayName);
                if (ProfileTask.Exception != null) // kalau update profil user ada error
                {
                    FbUser.DeleteAsync();
                    Debug.Log(ProfileTask.Exception);
                    FirebaseException firebaseException = ProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    string failMessage = "Profile update failed! Because ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failMessage += "Email is Invalid";
                            break;
                        case AuthError.WrongPassword:
                            failMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failMessage += "Email is missing, please provide email";
                            break;
                        case AuthError.MissingPassword:
                            failMessage += "Password is missing, please provide password";
                            break;
                        default:
                            failMessage = "Profile update failed :(";
                            break;
                    }
                    Debug.Log(failMessage);
                }

                else // kalau update user profile tidak ada error
                {
                    Debug.Log(FbUser.DisplayName);
                    Debug.Log("Registrasi berhasil bos"); // registrasi berhasil
                    //pindah ke LoginUI pakai coroutine

                    StartCoroutine(RegisterSuccess());
                }
            }
        }
    }

    private IEnumerator RegisterSuccess()
    {
        yield return new WaitForSeconds(0.8f);

        RegisterUI.SetActive(false);
        LoginUI.SetActive(true);
    }
    public void LoginButton()
    {
        StartCoroutine(LoginFirebase(Email.text, Password.text));
    }

    private IEnumerator LoginFirebase(string email, string password)
    {
        if (string.IsNullOrEmpty(email)) // kalau kosong (true), error
        {
            Debug.Log("email nya kosong bos");
        }

        else if (string.IsNullOrEmpty(password)) // kalau kosong (true), error
        {
            Debug.Log("password nya kosong");
        }

        else
        {
            Task<AuthResult> loginTask = FbAuth.SignInWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => loginTask.IsCompleted);

            if (loginTask.Exception != null) // kalau ada error
            {
                Debug.Log(loginTask.Exception);

                FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failMessage = "Login failed! Because ";

                switch (authError) // switch case - mirip if else if
                {
                    case AuthError.InvalidEmail:
                        failMessage += "Email is Invalid";
                        break;
                    case AuthError.WrongPassword:
                        failMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failMessage += "Email is missing, please provide email";
                        break;
                    case AuthError.MissingPassword:
                        failMessage += "Password is missing, please provide password";
                        break;
                    default:
                        failMessage = "Login failed, try again :(";
                        break;
                }
                Debug.Log(failMessage);
            }

            else
            {
                FbUser = loginTask.Result.User;
                Debug.Log("Login Success! " + FbUser.DisplayName);

                string username = FbUser.DisplayName;
                PlayerPrefs.SetString("myUsername", username);

                StartCoroutine(LoginSuccess());
            }
        }
    }

    private IEnumerator LoginSuccess()
    {
        yield return new WaitForSeconds(0.8f);

        LoginUI.SetActive(false);
        LoginUI.SetActive(true);
        Email.text = ""; // kosongin inputfield
        Password.text = ""; // kosongin inputfield

        string u = PlayerPrefs.GetString("myUsername");
        User.text = u;
        Debug.Log(User.text);
    }

    public void LogoutButton()
    {
        FbAuth.SignOut();

        StartCoroutine(LogoutFirebase());
    }

    private IEnumerator LogoutFirebase()
    {
        yield return new WaitForSeconds(1.2f);

        LoginUI.SetActive(false);
        MainmenuUI.SetActive(true);
    }

    void Update()
        
    {
        if (PasswordToggle.isOn) 
        {
            Password.contentType = TMP_InputField.ContentType.Standard;
            PasswordToggleText.text = "Hide Password";
        }

        else if (!PasswordToggle.isOn)
        {
            Password.contentType = TMP_InputField.ContentType.Password;
            PasswordToggleText.text = "Show Password";
        }

        Password.ForceLabelUpdate();

        if (PasswordRegisterToggle.isOn)
        {
            PasswordRegister.contentType = TMP_InputField.ContentType.Standard;
            ConfirmPass.contentType = TMP_InputField.ContentType.Standard;
            PasswordRegisterToggleText.text = "Hide Password";
        }

        else if (!PasswordRegisterToggle.isOn)
        {
            PasswordRegister.contentType = TMP_InputField.ContentType.Password;
            ConfirmPass.contentType = TMP_InputField.ContentType.Password;
            PasswordRegisterToggleText.text = "Show Password";
        }

        
        PasswordRegister.ForceLabelUpdate();
        ConfirmPass.ForceLabelUpdate();
    }
}
