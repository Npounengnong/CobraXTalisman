import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import Config from '../../../helpers/Config';
import { makeProductShortDescription } from '../../../helpers/ConversionHelper';
import rootAction from '../../../stateManagment/actions/rootAction';


const LoginUserModal = (props) => {
        
    const dispatch = useDispatch();
    const [adminPanelBaseURL, setadminPanelBaseURL] = useState(Config['ADMIN_BASE_URL']);
    const navigate = useNavigate();

    const loginUserDataJson = useSelector(state => state.userReducer.user);
    const loginUser = JSON.parse(loginUserDataJson ?? "{}");


const handleUpdateProfileUrl = (e)=>{
    props.handleOpenCloseLoginUserModal(e);
    navigate('/update-profile');
}

const handleOrderHistoryUrl = (e)=>{
    props.handleOpenCloseLoginUserModal(e);
    navigate('/orders-history');
}



    return (
        <>

            <div className={`bts-popup is-visible`} role="alert">
                <div className="bts-popup-container">
                    <h3>Hi {loginUser.FirstName} {loginUser.LastName}</h3>
                    <p>Wants to perform an action</p>


                    <div className='signup-form'>
                        <Link to="#" className="return-store"
                           onClick={(e) => {
                            e.preventDefault();
                            handleUpdateProfileUrl(e);
                        }}
                        >
                           1. Update Profile
                        </Link>
                        <br></br>

                        

                        <Link to="#" className="return-store"
                          onClick={(e) => {
                            e.preventDefault();
                            handleOrderHistoryUrl(e);
                        }}
                        
                        >
                       2. View Order History
                        </Link>

                      

                    </div>



                    <Link to="#" className="bts-popup-close"
                        onClick={(e) => {
                            props.handleOpenCloseLoginUserModal(e);
                        }}
                    >

                    </Link>
                </div>
            </div>

        </>
    );
}

export default LoginUserModal






