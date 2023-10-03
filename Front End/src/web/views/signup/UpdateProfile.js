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

const UpdateProfile = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [UserID, setUserID] = useState('');
    const [FirstName, setFirstName] = useState('');
    const [LastName, setLastName] = useState('');
    const [MobileNo, setMobileNo] = useState('');
    const [AddressLineOne, setAddressLineOne] = useState('');
    const [PostalCode, setPostalCode] = useState('');
    const [StateProvinceId, setStateProvinceId] = useState('');
    const [CityId, setCityId] = useState('');
    const [StatesProvincesList, setStatesProvincesList] = useState([]);
    const [CitiesList, setCitiesList] = useState([]);

    const loginUserDataJson = useSelector(state => state.userReducer.user);
    const loginUser = JSON.parse(loginUserDataJson ?? "{}");

    if (loginUser == null || loginUser == undefined || loginUser.UserID == undefined && loginUser.UserID < 1) {
        navigate('/');
    }


    const HandleStateProvinceChagne = async (value) => {
        if (value != undefined) {
            await setStateProvinceId(value);

            //--load city data when state changed
            await LoadCitiesData(value);
        }

    }

    const LoadCitiesData = async (StateValue) => {
        let headersCity = {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }


        let paramCity = {
            requestParameters: {
                StateProvinceId: StateValue ?? StateProvinceId,
                recordValueJson: "[]",
            },
        };

        //--Get cities list
        let responseCities = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_CITIES_LIST'], null, paramCity, headersCity, "POST", true);
        if (responseCities != null && responseCities.data != null) {
            await setCitiesList(JSON.parse(responseCities.data.data));

        }
    }


    const LoadStateProvinceData = async () => {
        let headersStateProvince = {
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }


        let paramStateProvince = {
            requestParameters: {
                recordValueJson: "[]",
            },
        };

        //--Get state province list
        let responseStatesProvince = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_STATES_PROVINCES_LIST'], null, paramStateProvince, headersStateProvince, "POST", true);
        if (responseStatesProvince != null && responseStatesProvince.data != null) {
            await setStatesProvincesList(JSON.parse(responseStatesProvince.data.data));

        }
    }

    const handleUpdateProfileForm = async (event) => {
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
                        UserID: UserID,
                        FirstName: FirstName,
                        LastName: LastName,
                        MobileNo: MobileNo,
                        AddressLineOne: AddressLineOne,
                        CityId: CityId,
                        StateProvinceId: StateProvinceId,
                        PostalCode: PostalCode

                    },
                };

                    


                //--make api call for data operation
                const response = await MakeApiCallAsync(Config.END_POINT_NAMES['UPDATE_PROFILE'], null, param, headers, "POST", true);
                if (response != null && response.data != null) {
                    let userData = JSON.parse(response.data.data);
                    if (userData.length > 0 && userData[0].ResponseMsg != undefined && userData[0].ResponseMsg == "Saved Successfully") {
                        showSuccessMsg("Profile Updated Successfully!");

                        loginUser.FirstName = FirstName;
                        loginUser.LastName = LastName;
                        loginUser.MobileNo = MobileNo;
                        loginUser.AddressLineOne = AddressLineOne;
                        loginUser.CityId = CityId;
                        loginUser.StateProvinceId = StateProvinceId;
                        loginUser.PostalCode = PostalCode;

                        //--save user data in session
                        localStorage.setItem("user", JSON.stringify(loginUser));
                        dispatch(rootAction.userAction.setUser(JSON.stringify(loginUser)));

                        setTimeout(() => {
                            navigate('/');
                        }, 1000);


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
        setUserID(loginUser.UserID);
        setFirstName(loginUser.FirstName);
        setLastName(loginUser.LastName);
        setMobileNo(loginUser.MobileNo);
        setAddressLineOne(loginUser.AddressLineOne);
        setStateProvinceId(loginUser.StateProvinceID);
        setCityId(loginUser.CityID);
        setPostalCode(loginUser.PostalCode);

    }, [])


    useEffect(() => {
        // declare the data fetching function
        const dataOperationInUseEffect = async () => {
            let headersCity = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            let paramCity = {
                requestParameters: {
                    StateProvinceId :loginUser.StateProvinceID ?? 0,
                    recordValueJson: "[]",
                },
            };

            //--Get cities list
            let responseCities = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_CITIES_LIST'], null, paramCity, headersCity, "POST", true);
            if (responseCities != null && responseCities.data != null) {
                await setCitiesList(JSON.parse(responseCities.data.data));

            }
           await setCityId(loginUser.CityID);


            let headersStateProvince = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            let paramStateProvince = {
                requestParameters: {
                    CountryId:  loginUser.CountryID ,
                    recordValueJson: "[]",
                },
            };


                
            //--Get state province list
            let responseStatesProvince = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_STATES_PROVINCES_LIST'], null, paramStateProvince, headersStateProvince, "POST", true);
            if (responseStatesProvince != null && responseStatesProvince.data != null) {
                await setStatesProvincesList(JSON.parse(responseStatesProvince.data.data));

            }
          await setStateProvinceId(loginUser.StateProvinceID);

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
                <title>{siteTitle} - Update Profile</title>
                <meta name="description" content={siteTitle + " - Update Profile"} />
                <meta name="keywords" content="Update Profile"></meta>
            </Helmet>
            <SiteBreadcrumb title="Update Profile" />

            <section className="signup-area ptb-60">
                <div className="container">
                    <div className="signup-content">
                        <div className="section-title">
                            <h2><span className="dot"></span> Update Profile</h2>
                        </div>

                        <form className="signup-form" onSubmit={handleUpdateProfileForm}>

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



                            <button type="submit" className="btn btn-primary">Update Profile</button>

                            <Link to="/" className="return-store">
                                Go To Home Page
                            </Link>
                        </form>
                    </div>
                </div>
            </section>

            <BestFacilities />


        </>
    );
}

export default UpdateProfile;
