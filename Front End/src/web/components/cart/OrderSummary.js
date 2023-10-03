import React, { useEffect, useState } from 'react';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import { useParams, Link, useNavigate } from 'react-router-dom';
import Config from '../../../helpers/Config';
import ProductsGridTypeOne from '../products/ProductsGridTypeOne';

import { useSelector, useDispatch } from 'react-redux';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';



const OrderSummary = () => {
    const dispatch = useDispatch();
    const [CartProductsList, setCartProductsList] = useState([]);
    const [CartSubTotal, setCartSubTotal] = useState(0);
    const [ShippingSubTotal, setShippingSubTotal] = useState(0);
    const [OrderTotal, setOrderTotal] = useState(0);



    const cartJsonDataSession = useSelector(state => state.cartReducer.cartItems);
    const cartItemsSession = JSON.parse(cartJsonDataSession ?? "[]");




    useEffect(() => {
        // declare the data fetching function
        const dataOperationInUseEffect = async () => {

             
            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                   
            }

            let ProductsIds = [];
            if (cartItemsSession != undefined && cartItemsSession != null && cartItemsSession.length > 0) {
                for (let i = 0; i < cartItemsSession.length; i++) {
                    ProductsIds.push({
                        ProductId: cartItemsSession[i].ProductId ?? 0
                    });
                }
            }


            const paramCountry = {
                requestParameters: {
                    ProductsIds: JSON.stringify(ProductsIds),
                    recordValueJson: "[]",
                },
            };


            //--Get products list agains a customer cart
            const responseProducts = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_PRODUCTS_LIST_BY_IDS'], null, paramCountry, headers, "POST", true);
            if (responseProducts != null && responseProducts.data != null) {
                let finalData = JSON.parse(responseProducts.data.data);
                await setCartProductsList(JSON.parse(responseProducts.data.data));

                let CartSubTotalDummy = 0;
                let ShippingSubTotalDummuy = 0;
                let OrderTotalDummu = 0;
                if (cartItemsSession != undefined && cartItemsSession != null && cartItemsSession.length > 0 && finalData.length>0) {
                    for (let i = 0; i < cartItemsSession.length; i++) {
                           
                        let productActualPrice = finalData?.find(x => x.ProductID == cartItemsSession[i].ProductId).Price;
                        let shippingCharges = finalData?.find(x => x.ProductID == cartItemsSession[i].ProductId).ShippingCharges;
                        let itemSubTotal = (productActualPrice ?? 0) * (cartItemsSession[i].Quantity ?? 1);

                        CartSubTotalDummy=CartSubTotalDummy + itemSubTotal;
                        ShippingSubTotalDummuy = ShippingSubTotalDummuy + (shippingCharges ?? 0);
                        OrderTotalDummu = OrderTotalDummu + (itemSubTotal + (shippingCharges ?? 0));
                       
                    }
                }
               await setCartSubTotal(CartSubTotalDummy);
               await setShippingSubTotal(ShippingSubTotalDummuy);
               await setOrderTotal(OrderTotalDummu);

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
            {
                CartProductsList != undefined && CartProductsList != null && CartProductsList.length > 0
                    ?
                    <>
                        <div className="order-table table-responsive">
                            <table className="table table-bordered">
                                <thead>
                                    <tr>
                                        <th scope="col">Product Name</th>
                                        <th scope="col">Quantity</th>
                                        <th scope="col">Total</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {cartItemsSession?.map((data, idx) => (

                                        <tr key={idx}>
                                            <td className="product-name">
                                                <Link to="#">
                                                    {data.ProductName}
                                                </Link>
                                            </td>
                                            <td className="product-name">
                                                {data.Quantity}
                                            </td>

                                            {(() => {
                                              
                                                let productActualPrice = CartProductsList?.find(x => x.ProductID == data.ProductId).Price;
                                                let shippingCharges = CartProductsList?.find(x => x.ProductID == data.ProductId).ShippingCharges;
                                                let itemSubTotal = (productActualPrice ?? 0) * (data.Quantity ?? 1);

                                                return (

                                                    <td className="product-total">
                                                        <span className="subtotal-amount">{GetDefaultCurrencySymbol()}{itemSubTotal}</span>
                                                    </td>
                                                );

                                            })()}


                                        </tr>
                                    ))}

                                    <tr>
                                        <td className="order-subtotal">
                                            <span>Cart Subtotal</span>
                                        </td>

                                        <td className="order-subtotal-price">
                                            <span className="order-subtotal-amount">${CartSubTotal}</span>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td className="order-shipping">
                                            <span>Shipping</span>
                                        </td>

                                        <td className="shipping-price">
                                            <span>{GetDefaultCurrencySymbol()}{ShippingSubTotal}</span>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td className="total-price">
                                            <span>Order Total</span>
                                        </td>

                                        <td className="product-subtotal">
                                            <span className="subtotal-amount">${OrderTotal}</span>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </>
                    :
                    <>
                    </>
            }

        </>
    );
}

export default OrderSummary;