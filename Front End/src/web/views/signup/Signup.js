import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import SiteBreadcrumb from '../../components/layout/SiteBreadcrumb';
import BestFacilities from '../../components/shared/BestFacilities';
import { useSelector, useDispatch } from 'react-redux';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { showErrorMsg, showSuccessMsg, validateAnyFormField } from '../../../helpers/ValidationHelper';
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import rootAction from '../../../stateManagment/actions/rootAction';
import { Helmet } from 'react-helmet';

const Signup = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [FirstName, setFirstName] = useState('');
    const [LastName, setLastName] = useState('');
    const [EmailAddress, setEmailAddress] = useState('');
    const [Password, setPassword] = useState('');
    const [MobileNo, setMobileNo] = useState('');
    const [AddressLineOne, setAddressLineOne] = useState('');
    const [StateProvinceId, setStateProvinceId] = useState('');
    const [CityId, setCityId] = useState('');
    const [PostalCode, setPostalCode] = useState('');
    const [CountryID, setCountryID] = useState('');
    const [CountriesList, setCountriesList] = useState([]);
    const [StatesProvincesList, setStatesProvincesList] = useState([]);
    const [CitiesList, setCitiesList] = useState([]);




    const HandleStateProvinceChagne = async (value) => {
        if (value != undefined) {
            await setStateProvinceId(value);

            //--load city data when state changed
            await LoadCitiesData(value);
        }

    }

    const HandleCountryChange = async (value) => {
        if (value != undefined) {
            await setCountryID(value);

            //--load state province data
            await LoadStateProvinceData(value);
        }

    }

    const LoadCitiesData = async (StateValue) => {
        const headersCity = {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }


        const paramCity = {
            requestParameters: {
                StateProvinceId: StateValue ?? StateProvinceId,
                recordValueJson: "[]",
            },
        };

        //--Get cities list
        const responseCities = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_CITIES_LIST'], null, paramCity, headersCity, "POST", true);
        if (responseCities != null && responseCities.data != null) {
            await setCitiesList(JSON.parse(responseCities.data.data));

        }
    }

    const LoadStateProvinceData = async (CountryValue) => {
        const headersStateProvince = {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }


        const paramStateProvince = {
            requestParameters: {
                CountryId: CountryValue ?? CountryID,
                recordValueJson: "[]",
            },
        };

        //--Get state province list
        const responseStatesProvince = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_STATES_PROVINCES_LIST'], null, paramStateProvince, headersStateProvince, "POST", true);
        if (responseStatesProvince != null && responseStatesProvince.data != null) {
            await setStatesProvincesList(JSON.parse(responseStatesProvince.data.data));

        }
    }
    const handleSignupForm = async (event) => {
        event.preventDefault();

        try {


            let isValid = false;
            let validationArray = [];


            isValid = validateAnyFormField('First Name', FirstName, 'text', null, 200, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }


            isValid = validateAnyFormField('Last Name', LastName, 'text', null, 150, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            isValid = validateAnyFormField('Email', EmailAddress, 'email', null, 150, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            isValid = validateAnyFormField('Password', Password, 'password', null, 150, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            isValid = validateAnyFormField('Country', CountryID, 'text', null, 150, true);
            if (isValid == false) {
                validationArray.push({
                    isValid: isValid
                });
            }

            isValid = validateAnyFormField('Shipping address', AddressLineOne, 'text', null, 600, true);
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

                const headers = {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                }



                    
                const param = {
                    requestParameters: {
                        FirstName: FirstName,
                        LastName: LastName,
                        EmailAddress: EmailAddress,
                        Password: Password,
                        MobileNo: MobileNo,
                        AddressLineOne: AddressLineOne,
                        CityId: CityId ?? -999,
                        StateProvinceId: StateProvinceId ?? -999,
                        PostalCode: PostalCode,
                        CountryID: CountryID ?? -999,
                    },
                };

                    debugger


                //--make api call for data operation
                const response = await MakeApiCallAsync(Config.END_POINT_NAMES['SIGNUP_USER'], null, param, headers, "POST", true);
                if (response != null && response.data != null) {
                    let userData = JSON.parse(response.data.data);
                    if (userData.length > 0 && userData[0].ResponseMsg != undefined && userData[0].ResponseMsg == "Saved Successfully") {
                        showSuccessMsg("Signup Successfully!");

                        //--CALL TO LOGIN API NOW TO LOGIN THE USER -- start here
                        const loginParam = {
                            requestParameters: {
                                Email: EmailAddress,
                                Password: Password
                            },
                        };

                        const responseLogin = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_USER_LOGIN'], null, loginParam, headers, "POST", true);
                        if (responseLogin != null && responseLogin.data != null) {
                            let loginData = JSON.parse(responseLogin.data.data);
                            if (loginData.length > 0 && loginData[0].ResponseMsg != undefined && loginData[0].ResponseMsg == "Login Successfully") {

                                //--save user data in session
                                localStorage.setItem("user", JSON.stringify(loginData[0]));
                                dispatch(rootAction.userAction.setUser(JSON.stringify(loginData[0])));

                                //--set Token in local storage
                                localStorage.setItem("Token", responseLogin.data.token ?? "");


                                navigate('/');

                            } else {
                                navigate('/login');
                            }
                        } else {
                            navigate('/login');
                        }
                        //--CALL TO LOGIN API NOW TO LOGIN THE USER -- ends here



                    } else if (userData.length > 0 && userData[0].ResponseMsg != undefined && userData[0].ResponseMsg == "Email already exists!") {
                        showErrorMsg("Email already exists!");
                        return false;
                    } else {
                        showErrorMsg("An error occured. Please try again!");
                        return false;
                    }
                }
            }



        } catch (err) {
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


    useEffect(() => {
        // declare the data fetching function
        const dataOperationInUseEffect = async () => {

            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const paramCountry = {
                requestParameters: {

                    recordValueJson: "[]",
                },
            };


            //--Get countreis list
            const responseCountries = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_COUNTRIES_LIST'], null, paramCountry, headers, "POST", true);
            if (responseCountries != null && responseCountries.data != null) {
                await setCountriesList(JSON.parse(responseCountries.data.data));

            }


        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        dataOperationInUseEffect().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);


    }, [])


    return (
        <>
            <Helmet>
                <title>{siteTitle} - Sign Up</title>
                <meta name="description" content={siteTitle + " - Sign Up"} />
                <meta name="keywords" content="Sign Up"></meta>
            </Helmet>
            <SiteBreadcrumb title="Signup" />

            <section className="signup-area ptb-60">
                <div className="container">
                    <div className="signup-content">
                        <div className="section-title">
                            <h2><span className="dot"></span> Create an Account</h2>
                        </div>

                        <form className="signup-form" onSubmit={handleSignupForm}>

                            <div className='row'>
                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>First Name <span className="required-field">*</span></label>
                                        <input type="text" className="form-control" placeholder="Enter first name" id="FirstName" name="FirstName"
                                            required={true}
                                            value={FirstName}
                                            onChange={(e) => setFirstName(e.target.value)}
                                        />
                                    </div>
                                </div>

                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>Last Name <span className="required-field">*</span></label>
                                        <input type="text" className="form-control" placeholder="Enter last name" id="LastName" name="LastName"
                                            required={true}
                                            value={LastName}
                                            onChange={(e) => setLastName(e.target.value)}
                                        />
                                    </div>
                                </div>

                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>Email <span className="required-field">*</span></label>
                                        <input type="email" className="form-control" placeholder="Enter email" id="EmailAddress" name="EmailAddress"
                                            required={true}
                                            value={EmailAddress}
                                            onChange={(e) => setEmailAddress(e.target.value)}
                                        />
                                    </div>
                                </div>

                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>Country <span className="required-field">*</span></label>
                                        <div className="select-box">
                                            <select
                                                className="form-control"
                                                name="CountryID"
                                                required={true}
                                                value={CountryID}
                                                onChange={(e) => HandleCountryChange(e.target.value)}
                                            >
                                                <option value="-999">Select Country</option>
                                                {
                                                    CountriesList?.map((item, idx) =>

                                                        <option value={item.CountryID}>{item.CountryName}</option>

                                                    )}


                                            </select>
                                        </div>
                                    </div>
                                </div>


                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>State/Province </label>
                                        <div className="select-box">
                                            <select
                                                className="form-control"
                                                name="StateProvinceId"
                                                required={false}
                                                value={StateProvinceId}
                                                onChange={(e) => HandleStateProvinceChagne(e.target.value)}
                                            >
                                                <option value="-999">Select State</option>
                                                {
                                                    StatesProvincesList?.map((item, idx) =>

                                                        <option value={item.StateProvinceID}>{item.StateName}</option>

                                                    )}


                                            </select>
                                        </div>
                                    </div>
                                </div>

                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>City </label>
                                        <div className="select-box">
                                            <select
                                                className="form-control"
                                                name="CityId"
                                                required={false}
                                                value={CityId}
                                                onChange={(e) => setCityId(e.target.value)}
                                            >
                                                <option value="-999">Select City</option>
                                                {
                                                    CitiesList?.map((item, idx) =>

                                                        <option value={item.CityID}>{item.CityName}</option>

                                                    )}


                                            </select>
                                        </div>
                                    </div>
                                </div>


                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>Contact No </label>
                                        <input type="text" className="form-control" placeholder="Enter contact no" id="MobileNo" name="MobileNo"
                                            value={MobileNo}
                                            onChange={(e) => setMobileNo(e.target.value)}
                                        />
                                    </div>
                                </div>


                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>Postal Code </label>
                                        <input type="number" className="form-control" placeholder="Enter postal code" id="PostalCode" name="PostalCode"
                                            value={PostalCode}
                                            onChange={(e) => setPostalCode(e.target.value)}
                                        />
                                    </div>
                                </div>

                                <div className='col-lg-6 col-md-6'>
                                    <div className="form-group">
                                        <label>Password</label>
                                        <input type="password" className="form-control" placeholder="Enter your password" id="Password" name="Password"
                                            required={true}
                                            value={Password}
                                            onChange={(e) => setPassword(e.target.value)}
                                        />
                                    </div>
                                </div>

                                <div className='col-lg-12 col-md-12'>
                                    <div className="form-group">
                                        <label>Shipping Address <span className="required-field">*</span></label>
                                        <input type="text" className="form-control" placeholder="Enter shipping address" id="AddressLineOne" name="AddressLineOne"
                                            required={true}
                                            value={AddressLineOne}
                                            onChange={(e) => setAddressLineOne(e.target.value)}
                                        />
                                    </div>
                                </div>

                            </div>



                            <button type="submit" className="btn btn-primary">Signup</button>

                            <Link to="/login" className="return-store">
                                or Login to your account
                            </Link>
                        </form>
                    </div>
                </div>
            </section>

            <BestFacilities />


        </>
    );
}

export default Signup;
