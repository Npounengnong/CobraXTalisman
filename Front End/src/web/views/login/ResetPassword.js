import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import SiteBreadcrumb from '../../components/layout/SiteBreadcrumb';
import BestFacilities from '../../components/shared/BestFacilities';
import rootAction from '../../../stateManagment/actions/rootAction';
import Config from '../../../helpers/Config';
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import { showErrorMsg, showSuccessMsg, validateAnyFormField } from '../../../helpers/ValidationHelper';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { Helmet } from 'react-helmet';

const ResetPassword = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);

    const [displayRecoverEmailForm, setdisplayRecoverEmailForm] = useState(true);
    const [displayOTPForm, setdisplayOTPForm] = useState(false);

    const [Email, setEmail] = useState('');
    const [Otp, setOtp] = useState('');
    const [Password, setPassword] = useState('');
    const [ConfirmPassword, setConfirmPassword] = useState('');




    const submitEmailSendOTPForm = async (event) => {
        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));


        try {

            event.preventDefault();

            let isValid = false;
            let validationArray = [];

            //--validation for email
            isValid = validateAnyFormField('Email', Email, 'email', null, 200, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }


            //--check if any field is not valid
            if (validationArray != null && validationArray.length > 0) {

                isValid = false;
                return false;
            } else {
                isValid = true;
            }

            if (isValid) {

                let headersEmailSendOTP = {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                }


                let paramEmailSendOTP = {
                    requestParameters: {
                        Email: Email
                    },
                };

                //--make api call for data operation
                let responseEmailSendOTP = await MakeApiCallAsync(Config.END_POINT_NAMES['VALIDATE_EMAIL_SEND_OTP'],Config["COMMON_CONTROLLER_SUB_URL"], paramEmailSendOTP, headersEmailSendOTP, "POST", true);
                   
                if (responseEmailSendOTP != null && responseEmailSendOTP.data.statusCode != null && responseEmailSendOTP.data.statusCode == 200 && responseEmailSendOTP.data.message == "Sent Successfully") {
                    showSuccessMsg("An OTP has been sent to your email. Please confirm OTP & enter new password!");
                    await setdisplayRecoverEmailForm(false);
                    await setdisplayOTPForm(true);

                } else {
                    showErrorMsg(responseEmailSendOTP.data.errorMessage);
                    return false;
                }
            }
        }
        catch (err) {
            console.log(err);
            showErrorMsg("An error occured. Please try again!");

            return false;
        } finally {
            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);

        }


    }


    const validateOTPAndChangePassword = async (event) => {
        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));


        try {

            event.preventDefault();

            let isValid = false;
            let validationArray = [];

            //--validation for email
            isValid = validateAnyFormField('Email', Email, 'email', null, 200, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            isValid = validateAnyFormField('Password', Password, 'password', 6, 20, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            isValid = validateAnyFormField('ConfirmPassword', ConfirmPassword, 'password', 6, 20, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }


            isValid = validateAnyFormField('OTP', Otp, 'text', 5, 15, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            if (Password != ConfirmPassword) {
                showErrorMsg("Password does not match!");
                validationArray.push({
                    isValid: false
                });
            }

            //--check if any field is not valid
            if (validationArray != null && validationArray.length > 0) {

                isValid = false;
                return false;
            } else {
                isValid = true;
            }

            if (isValid) {

                let headersChangePassword = {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                }


                let paramChangePassword = {
                    requestParameters: {
                        Email: Email,
                        Otp: Otp,
                        Password: Password,
                        ConfirmPassword: ConfirmPassword,
                    },
                };

                //--make api call for data operation
                let responseChangePassword = await MakeApiCallAsync(Config.END_POINT_NAMES['VALIDATE_OTP_CHANGE_PASSWORD'],Config["COMMON_CONTROLLER_SUB_URL"], paramChangePassword, headersChangePassword, "POST", true);
               
                if (responseChangePassword != null && responseChangePassword.data.statusCode != null && responseChangePassword.data.statusCode == 200 && responseChangePassword.data.message == "Password reset successfully") {
                    showSuccessMsg("Password changed successfully. Please login with your new password!");
                  

                    navigate('/login');

                } else {
                    showErrorMsg(responseChangePassword.data.errorMessage);
                    return false;
                }
            }
        }
        catch (err) {
            console.log(err);
            showErrorMsg("An error occured. Please try again!");

            return false;
        } finally {
            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);

        }


    }

    return (
        <>
            <Helmet>
                <title>{siteTitle} - Reset Password</title>
                <meta name="description" content={siteTitle + " - Reset Password"} />
                <meta name="keywords" content="Reset Password, Change Password"></meta>
            </Helmet>

            <SiteBreadcrumb title="Reset Password" />

            <section className="login-area ptb-60">
                <div className="container">
                    <div className="row">
                        <div className="col-lg-6 col-md-12">
                            <div className="login-content">
                                <div className="section-title">
                                    <h2><span className="dot"></span> Reset Password</h2>
                                </div>

                                <form className="login-form" id="password_recovery_form" onSubmit={submitEmailSendOTPForm} style={{ display: displayRecoverEmailForm == true ? 'block' : 'none' }}>
                                    <div className="form-group">
                                        <label>Email</label>
                                        <input type="email" className="form-control" placeholder="Enter your email" id="name" name="name"
                                            required={true}
                                            value={Email}
                                            onChange={(e) => setEmail(e.target.value)}
                                        />
                                    </div>


                                    <button type="submit" className="btn btn-primary">Recover Password</button>

                                    <Link to="/login" className="forgot-password">
                                        Login
                                    </Link>
                                </form>



                                <form className="login-form" id="otp_new_password_form" onSubmit={validateOTPAndChangePassword} style={{ display: displayOTPForm == true ? 'block' : 'none' }}>
                                    <div className="form-group">
                                        <label>OTP</label>
                                        <input type="number" className="form-control" placeholder="Enter OTP" id="Otp" name="Otp"
                                            required={true}
                                            value={Otp}
                                            onChange={(e) => setOtp(e.target.value)}
                                        />
                                    </div>

                                    <div className="form-group">
                                        <label>Password</label>
                                        <input type="password" className="form-control" placeholder="Enter new password" id="password" name="password"
                                            required={true}
                                            value={Password}
                                            onChange={(e) => setPassword(e.target.value)}
                                        />
                                    </div>

                                    <div className="form-group">
                                        <label>Confirm Password</label>
                                        <input type="password" className="form-control" placeholder="Confirm password" id="ConfirmPassword" name="ConfirmPassword"
                                            required={true}
                                            value={ConfirmPassword}
                                            onChange={(e) => setConfirmPassword(e.target.value)}
                                        />
                                    </div>

                                    <button type="submit" className="btn btn-primary">Change Password</button>

                                    <Link to="/login" className="forgot-password">
                                        Login
                                    </Link>
                                </form>



                            </div>
                        </div>

                        <div className="col-lg-6 col-md-12">
                            <div className="new-customer-content">
                                <div className="section-title">
                                    <h2><span className="dot"></span> New Customer</h2>
                                </div>

                                <span>Create a Account</span>
                                <p>Sign up for a free account at our store. Registration is quick and easy. It allows you to be able to order from our shop. To start shopping click register.</p>
                                <Link to="/signup" className="btn btn-light">
                                    Create A Account

                                </Link>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <BestFacilities />


        </>
    );
}

export default ResetPassword;