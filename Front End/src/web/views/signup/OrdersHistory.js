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
import { convertDateToDifferentFormats, makeProductShortDescription } from '../../../helpers/ConversionHelper';
import { Helmet } from 'react-helmet';
import { GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';


const OrdersHistory = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [adminPanelBaseURL, setadminPanelBaseURL] = useState(Config['ADMIN_BASE_URL']);
    const [OrderMasterList, setOrderMasterList] = useState([]);
    const [OrderItemsDetailList, setOrderItemsDetailList] = useState([]);

    //-get login user from session
    const loginUserDataJson = useSelector(state => state.userReducer.user);
    const loginUser = JSON.parse(loginUserDataJson ?? "{}");


    const viewOrderItemsDetails = async (OrderId) => {
        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));


        const headersDetail = {
            // customerid: userData?.UserID,
            // customeremail: userData.EmailAddress,
            Accept: 'application/json',
            'Content-Type': 'application/json',
        }


        const paramDetail = {
            requestParameters: {
                OrderId: OrderId,
                recordValueJson: "[]",
            },
        };


        let responseDetailOrderDetail = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_CUSTOME_ORDER_HISTORY_DETAIL'], null, paramDetail, headersDetail, "POST", true);
        if (responseDetailOrderDetail != null && responseDetailOrderDetail.data != null) {
            await setOrderItemsDetailList(JSON.parse(responseDetailOrderDetail.data.data));
            console.log(JSON.parse(responseDetailOrderDetail.data.data));
        }


        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);

        try {
            document.getElementById('order_item_detail').scrollIntoView({
                behavior: 'smooth'
            });
        }
        catch (err) {

            console.log(err.message);
        }


    }

    useEffect(() => {
        // declare the data fetching function
        const getOrderHistoryMaster = async () => {

            const headers = {
                // customerid: userData?.UserID,
                // customeremail: userData.EmailAddress,
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const param = {
                requestParameters: {
                    UserId: loginUser.UserID,
                    recordValueJson: "[]",
                },
            };


            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_CUSTOMER_ORDER_HISTORY_MASTER'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                await setOrderMasterList(JSON.parse(response.data.data));
                console.log(JSON.parse(response.data.data));
            }


        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        getOrderHistoryMaster().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);

    }, [])



    return (
        <>
            <Helmet>
                <title>{siteTitle} - Orders History</title>
                <meta name="description" content={siteTitle + " Orders History"} />
                <meta name="keywords" content="Orders History"></meta>
            </Helmet>

            <SiteBreadcrumb title="Orders History" />

            <section className="cart-area ptb-60">



                <div className="container">
                    <div className="row">
                        {
                            OrderMasterList != undefined && OrderMasterList != null && OrderMasterList.length > 0
                                ?
                                <>
                                    <div className="col-lg-12 col-md-12">
                                        <form>
                                            <div className="cart-table table-responsive">
                                                <table className="table table-bordered">
                                                    <thead>
                                                        <tr>
                                                            <th scope="col">Order Number</th>
                                                            <th scope="col">Order Date</th>
                                                            <th scope="col">Status</th>
                                                            <th scope="col">Total Items</th>
                                                            <th scope="col">Total</th>
                                                            <th scope="col">Detail</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {OrderMasterList?.map((item, idx) => (
                                                            <tr key={idx}>


                                                                <td className="product-name"> {item.OrderNumber}</td>
                                                                <td className="product-name"> {convertDateToDifferentFormats(item.OrderDateUTC, 'dd-mm-yyyy')}</td>

                                                                <td className="product-price">
                                                                    <span className="unit-amount">
                                                                        {item.LatestStatusName}

                                                                    </span>

                                                                </td>

                                                                <td>
                                                                    <span> {item.TotalItems}</span>

                                                                </td>

                                                                <td className="product-subtotal">
                                                                    <span className="subtotal-amount">{GetDefaultCurrencySymbol()}{item.OrderTotal}</span>
                                                                </td>
                                                                <td>
                                                                    <Link to="#"
                                                                        className="remove"
                                                                        onClick={() => { viewOrderItemsDetails(item.OrderId) }}
                                                                    >
                                                                        <i className="far fa-eye"></i>
                                                                    </Link>
                                                                </td>
                                                            </tr>
                                                        ))}
                                                    </tbody>
                                                </table>
                                            </div>

                                            <div className="cart-buttons">
                                                <div className="row align-items-center">
                                                    <div className="col-lg-7 col-md-7">
                                                        <div className="continue-shopping-box">
                                                            <Link to="/" className="btn btn-light">
                                                                Continue Shopping
                                                            </Link>
                                                        </div>
                                                    </div>

                                                    {/* <div className="col-lg-5 col-md-5 text-right">
                                                        <label>
                                                            <input
                                                                type="checkbox"
                                                                ref="shipping"
                                                                onChange={this.handleChecked}
                                                            />
                                                            <span>Shipping(+6$)</span>
                                                        </label>
                                                    </div> */}
                                                </div>
                                            </div>

                                            <div id="order_item_detail" className="cart-totals" style={{ maxWidth: "100%" }}>
                                                <h3>Cart Items</h3>

                                                <table className="table table-bordered">
                                                    <thead>
                                                        <tr>
                                                            <th scope="col">Product</th>
                                                            <th scope="col">Name</th>

                                                            <th scope="col">Unit Price</th>
                                                            <th scope="col">Quantity</th>
                                                            <th scope="col">Item Total</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>

                                                        {OrderItemsDetailList?.map((itemDetail, idx) => (
                                                            <tr key={idx}>
                                                                <td className="product-thumbnail">
                                                                    <Link to="#">
                                                                        <img src={adminPanelBaseURL + itemDetail.DefaultImageUrl} alt="image" width={100} height={90} />
                                                                        {/* <img src="https://localhost:7248//content/commonImages/productImages/811_instagram3.jpg" alt="image" /> */}


                                                                    </Link>
                                                                </td>

                                                                <td className="product-name">{itemDetail.ProductName}</td>

                                                                <td className="product-price">{GetDefaultCurrencySymbol()}{itemDetail.Price}</td>
                                                                <td className="product-quantity">
                                                                    <div className="input-counter">
                                                                        {itemDetail.Quantity}

                                                                    </div>
                                                                </td>
                                                                <td>{(itemDetail.OrderItemTotal)}</td>
                                                            </tr>
                                                        ))}

                                                    </tbody>
                                                </table>

                                                {/* <Link to="/checkout" className="btn btn-light">
                                                    Proceed to Checkout
                                                </Link> */}
                                            </div>
                                        </form>
                                    </div>
                                </>
                                :
                                <>

                                </>
                        }




                    </div>
                </div>
            </section>


            <BestFacilities />


        </>
    );
}

export default OrdersHistory;
