import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import Config from '../../../helpers/Config';
import { makeProductShortDescription } from '../../../helpers/ConversionHelper';
import rootAction from '../../../stateManagment/actions/rootAction';
import { GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';


const Wishlist = (props) => {
    const dispatch = useDispatch();
    const [adminPanelBaseURL, setadminPanelBaseURL] = useState(Config['ADMIN_BASE_URL']);
    const navigate = useNavigate();

    const jsoncustomerWishList = useSelector(state => state.cartReducer.customerWishList);
    const wishListData = JSON.parse(jsoncustomerWishList ?? "[]");
    const wishListCount = wishListData != undefined && wishListData != null ? wishListData.length : 0;


 const handleContinueShopping =()=>{
    props.handleWishlist();
      setTimeout(() => {
        navigate('/');
    }, 500);
 }

 const makeEmptyFromWishList =()=>{


    localStorage.setItem("customerWishList", '[]');
    dispatch(rootAction.cartAction.setCustomerWishList('[]'));

    props.handleWishlist();
     
 }


 


    return (
        <>

            {
                wishListData != undefined ?
                    <>
                        <div
                            className="modal right fade show shoppingCartModal"
                            style={{
                                display: "block", paddingRight: "16px"
                            }}
                        >
                            <div className="modal-dialog" role="document">
                                <div className="modal-content">
                                    <button
                                        type="button"
                                        className="close"
                                        data-dismiss="modal"
                                        aria-label="Close"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            props.handleWishlist();
                                        }}

                                    >
                                        <span aria-hidden="true">&times;</span>
                                    </button>

                                    <div className="modal-body">
                                        <h3>My Wish List ({wishListCount ?? 0})</h3>

                                        <div className="product-cart-content">

                                            {wishListData?.map((item, idx) => (
                                                <div className="product-cart" key={idx}>
                                                    <div className="product-image">
                                                        <img src={item.DefaultImage != undefined ? (adminPanelBaseURL + item.DefaultImage) : ""} alt="image" />
                                                    </div>

                                                    <div className="product-content">
                                                        <h3>
                                                            <Link to="#">
                                                                {
                                                                    makeProductShortDescription(item.ProductName, 30)
                                                                }

                                                            </Link>
                                                        </h3>
                                                        {/* <span>Blue / XL</span> */}
                                                        <div className="product-price">
                                                            <span>{item.Quantity}</span>
                                                            <span>x</span>
                                                            <span className="price">{GetDefaultCurrencySymbol()}{item.Price}</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}

                                        </div>

                                        <div className="product-cart-btn">
                                            <Link to="#" className="btn btn-primary"
                                             onClick={(e) => {
                                                e.preventDefault();
                                                handleContinueShopping();
                                            }}
                                            >
                                                Continue Shopping
                                            </Link>

                                            <Link to="#" className="btn btn-light"
                                             onClick={(e) => {
                                                e.preventDefault();
                                                makeEmptyFromWishList();
                                            }}
                                            >
                                               Clear Wishlist
                                            </Link>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>
                    </>
                    :

                    <>

                    </>
            }



        </>
    );
}

export default Wishlist






// import React, { Component } from 'react';

// import Link from 'next/link';

// class Wishlist extends Component {

//     state = {
//         display: false
//     };

//     closeWishlist = () => {
//         this.props.onClick(this.state.display);
//     }

//     render() {
//         return (
//             <div
//                 className="modal right fade show shoppingWishlistModal"
//                 style={{
//                     display: "block", paddingRight: "16px"
//                 }}
//             >
//                 <div className="modal-dialog" role="document">
//                     <div className="modal-content">
//                         <button
//                             type="button"
//                             className="close"
//                             data-dismiss="modal"
//                             aria-label="Close"
//                             onClick={this.closeWishlist}
//                         >
//                             <span aria-hidden="true">&times;</span>
//                         </button>

//                         <div className="modal-body">
//                             <h3>My Wish List (3)</h3>

//                             <div className="product-cart-content">
//                                 <div className="product-cart">
//                                     <div className="product-image">
//                                         <img src="/images/img2.jpg" alt="image" />
//                                     </div>

//                                     <div className="product-content">
//                                         <h3>
//                                             <Link href="#">
//                                                 <a>Belted chino trousers polo</a>
//                                             </Link>
//                                         </h3>
//                                         <span>Blue / XS</span>
//                                         <div className="product-price">
//                                             <span>1</span>
//                                             <span>x</span>
//                                             <span className="price">$191.00</span>
//                                         </div>
//                                     </div>
//                                 </div>

//                                 <div className="product-cart">
//                                     <div className="product-image">
//                                         <img src="/images/img3.jpg" alt="image" />
//                                     </div>

//                                     <div className="product-content">
//                                         <h3>
//                                             <Link href="#">
//                                                 <a>Belted chino trousers polo</a>
//                                             </Link>
//                                         </h3>
//                                         <span>Blue / XS</span>
//                                         <div className="product-price">
//                                             <span>1</span>
//                                             <span>x</span>
//                                             <span className="price">$191.00</span>
//                                         </div>
//                                     </div>
//                                 </div>

//                                 <div className="product-cart">
//                                     <div className="product-image">
//                                         <img src="/images/img4.jpg" alt="image" />
//                                     </div>

//                                     <div className="product-content">
//                                         <h3>
//                                             <Link href="#">
//                                                 <a>Belted chino trousers polo</a>
//                                             </Link>
//                                         </h3>
//                                         <span>Blue / XS</span>
//                                         <div className="product-price">
//                                             <span>1</span>
//                                             <span>x</span>
//                                             <span className="price">$191.00</span>
//                                         </div>
//                                     </div>
//                                 </div>
//                             </div>

//                             <div className="product-cart-btn">
//                                 <Link href="#">
//                                     <a className="btn btn-light">View Shopping Cart</a>
//                                 </Link>
//                             </div>
//                         </div>
//                     </div>
//                 </div>
//             </div>
//         );
//     }
// }

// export default Wishlist;





