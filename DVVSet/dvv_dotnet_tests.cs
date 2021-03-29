using System.Collections.Generic;
using NUnit.Framework;

namespace DVVSet.Tests
{
    [TestFixture]
    public class TestDvvSet
    {
        [TestCase]
        public void Test_join()
        {
            var a = new Clock("v1");
            var a1 = Clock.Create(a, "a");

            var b = this.dvvset.new_with_history(this.dvvset.join(a1), "v2");
            var b1 = this.dvvset.update(b, a1, "b");
            Assert.AreEqual(this.dvvset.join(a), new List<object>());
            Assert.AreEqual(this.dvvset.join(a1), new List<object> {
                    new List<object> {
                        "a",
                        1
                    }
                });
            this.assertEqual(this.dvvset.join(b1), new List<object> {
                    new List<object> {
                        "a",
                        1
                    },
                    new List<object> {
                        "b",
                        1
                    }
                });
            Assert.AreEqual(0, clock.Versions.Count);
        }

        public virtual object test_update()
        {
            var a0 = this.dvvset.create(this.dvvset.@new("v1"), "a");
            var a1 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(a0), new List<object> {
                    "v2"
                }), a0, "a");
            var a2 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(a1), new List<object> {
                    "v3"
                }), a1, "b");
            var a3 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(a0), new List<object> {
                    "v4"
                }), a1, "b");
            var a4 = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(a0), new List<object> {
                    "v5"
                }), a1, "a");
            this.assertEqual(a0, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(a1, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(a2, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(a3, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2"
                            }
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v4"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(a4, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            3,
                            new List<object> {
                                "v5",
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
        }

        public virtual object test_sync()
        {
            var x = new List<object> {
                    new List<object> {
                        new List<object> {
                            "x",
                            1,
                            new List<object>()
                        }
                    },
                    new List<object>()
                };
            var a = this.dvvset.create(this.dvvset.@new("v1"), "a");
            var y = this.dvvset.create(this.dvvset.new_list(new List<object> {
                    "v2"
                }), "b");
            var a1 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(a), new List<object> {
                    "v2"
                }), "a");
            var a3 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(a1), new List<object> {
                    "v3"
                }), "b");
            var a4 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(a1), new List<object> {
                    "v3"
                }), "c");
            var w = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object>()
                        }
                    },
                    new List<object>()
                };
            var z = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2",
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                };
            this.assertEqual(this.dvvset.sync(new List<object> {
                    w,
                    z
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(this.dvvset.sync(new List<object> {
                    w,
                    z
                }), this.dvvset.sync(new List<object> {
                    z,
                    w
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    a,
                    a1
                }), this.dvvset.sync(new List<object> {
                    a1,
                    a
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    a4,
                    a3
                }), this.dvvset.sync(new List<object> {
                    a3,
                    a4
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    a4,
                    a3
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v3"
                            }
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(this.dvvset.sync(new List<object> {
                    x,
                    a
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        },
                        new List<object> {
                            "x",
                            1,
                            new List<object>()
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(this.dvvset.sync(new List<object> {
                    x,
                    a
                }), this.dvvset.sync(new List<object> {
                    a,
                    x
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    x,
                    a
                }), this.dvvset.sync(new List<object> {
                    a,
                    x
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    a,
                    y
                }), new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        },
                        new List<object> {
                            "b",
                            1,
                            new List<object> {
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(this.dvvset.sync(new List<object> {
                    y,
                    a
                }), this.dvvset.sync(new List<object> {
                    a,
                    y
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    y,
                    a
                }), this.dvvset.sync(new List<object> {
                    a,
                    y
                }));
            this.assertEqual(this.dvvset.sync(new List<object> {
                    a,
                    x
                }), this.dvvset.sync(new List<object> {
                    x,
                    a
                }));
        }

        public virtual object test_sync_update()
        {
            // Mary writes v1 w/o VV
            var a0 = this.dvvset.create(this.dvvset.new_list(new List<object> {
                    "v1"
                }), "a");
            // Peter reads v1 with version vector (VV)
            var vv1 = this.dvvset.join(a0);
            // Mary writes v2 w/o VV
            var a1 = this.dvvset.update(this.dvvset.new_list(new List<object> {
                    "v2"
                }), a0, "a");
            // Peter writes v3 with VV from v1
            var a2 = this.dvvset.update(this.dvvset.new_list_with_history(vv1, new List<object> {
                    "v3"
                }), a1, "a");
            this.assertEqual(vv1, new List<object> {
                    new List<object> {
                        "a",
                        1
                    }
                });
            this.assertEqual(a0, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            1,
                            new List<object> {
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                });
            this.assertEqual(a1, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            2,
                            new List<object> {
                                "v2",
                                "v1"
                            }
                        }
                    },
                    new List<object>()
                });
            // now A2 should only have v2 and v3, since v3 was causally newer than v1
            this.assertEqual(a2, new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            3,
                            new List<object> {
                                "v3",
                                "v2"
                            }
                        }
                    },
                    new List<object>()
                });
        }

        public virtual object test_event()
        {
            new List<object> {
                    A,
                    _
                } = this.dvvset.create(this.dvvset.@new("v1"), "a");
            this.assertEqual(this.dvvset.@event(A, "a", "v2"), new List<object> {
                    new List<object> {
                        "a",
                        2,
                        new List<object> {
                            "v2",
                            "v1"
                        }
                    }
                });
            this.assertEqual(this.dvvset.@event(A, "b", "v2"), new List<object> {
                    new List<object> {
                        "a",
                        1,
                        new List<object> {
                            "v1"
                        }
                    },
                    new List<object> {
                        "b",
                        1,
                        new List<object> {
                            "v2"
                        }
                    }
                });
        }

        public virtual object test_less()
        {
            var a = this.dvvset.create(this.dvvset.new_list("v1"), new List<object> {
                    "a"
                });
            var b = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(a), new List<object> {
                    "v2"
                }), "a");
            var b2 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(a), new List<object> {
                    "v2"
                }), "b");
            var b3 = this.dvvset.create(this.dvvset.new_list_with_history(this.dvvset.join(a), new List<object> {
                    "v2"
                }), "z");
            var c = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(b), new List<object> {
                    "v3"
                }), a, "c");
            var d = this.dvvset.update(this.dvvset.new_list_with_history(this.dvvset.join(c), new List<object> {
                    "v4"
                }), b2, "d");
            this.assertTrue(this.dvvset.less(a, b));
            this.assertTrue(this.dvvset.less(a, c));
            this.assertTrue(this.dvvset.less(b, c));
            this.assertTrue(this.dvvset.less(b, d));
            this.assertTrue(this.dvvset.less(b2, d));
            this.assertTrue(this.dvvset.less(a, d));
            this.assertFalse(this.dvvset.less(b2, c));
            this.assertFalse(this.dvvset.less(b, b2));
            this.assertFalse(this.dvvset.less(b2, b));
            this.assertFalse(this.dvvset.less(a, a));
            this.assertFalse(this.dvvset.less(c, c));
            this.assertFalse(this.dvvset.less(d, b2));
            this.assertFalse(this.dvvset.less(b3, d));
        }

        public virtual object test_equal()
        {
            var a = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v5",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    },
                    new List<object> {
                        "c",
                        1,
                        new List<object> {
                            "v3"
                        }
                    }
                }, new List<object> {
                    "v0"
                });
            var b = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v555",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    },
                    new List<object> {
                        "c",
                        1,
                        new List<object> {
                            "v3"
                        }
                    }
                }, new List<object>());
            var c = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v5",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    }
                }, new List<object> {
                    "v6",
                    "v1"
                });
            // compare only the causal history
            this.assertTrue(this.dvvset.equal(a, b));
            this.assertTrue(this.dvvset.equal(b, a));
            this.assertFalse(this.dvvset.equal(a, c));
            this.assertFalse(this.dvvset.equal(b, c));
        }

        public virtual object test_size()
        {
            Tuple.Create(this.assertEqual(1, this.dvvset.size(this.dvvset.new_list(new List<object> {
                    "v1"
                }))));
            var clock = Clock(new List<object> {
                    new List<object> {
                        "a",
                        4,
                        new List<object> {
                            "v5",
                            "v0"
                        }
                    },
                    new List<object> {
                        "b",
                        0,
                        new List<object>()
                    },
                    new List<object> {
                        "c",
                        1,
                        new List<object> {
                            "v3"
                        }
                    }
                }, new List<object> {
                    "v4",
                    "v1"
                });
            this.assertEqual(5, this.dvvset.size(clock));
        }

        public virtual object test_values()
        {
            var a = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v5"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object> {
                        "v1"
                    }
                };
            var b = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v555"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                };
            var c = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        }
                    },
                    new List<object> {
                        "v1",
                        "v6"
                    }
                };
            this.assertEqual(this.dvvset.ids(a), new List<object> {
                    "a",
                    "b",
                    "c"
                });
            this.assertEqual(this.dvvset.ids(b), new List<object> {
                    "a",
                    "b",
                    "c"
                });
            this.assertEqual(this.dvvset.ids(c), new List<object> {
                    "a",
                    "b"
                });
            this.assertEqual(this.dvvset.values(a).OrderBy(p1 => p1).ToList(), new List<object> {
                    "v0",
                    "v1",
                    "v3",
                    "v5"
                });
            this.assertEqual(this.dvvset.values(b).OrderBy(p2 => p2).ToList(), new List<object> {
                    "v0",
                    "v3",
                    "v555"
                });
            this.assertEqual(this.dvvset.values(c).OrderBy(p3 => p3).ToList(), new List<object> {
                    "v1",
                    "v6"
                });
        }

        public virtual object test_ids_values()
        {
            var a = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v5"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object> {
                        "v1"
                    }
                };
            var b = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object> {
                                "v0",
                                "v555"
                            }
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        },
                        new List<object> {
                            "c",
                            1,
                            new List<object> {
                                "v3"
                            }
                        }
                    },
                    new List<object>()
                };
            var c = new List<object> {
                    new List<object> {
                        new List<object> {
                            "a",
                            4,
                            new List<object>()
                        },
                        new List<object> {
                            "b",
                            0,
                            new List<object>()
                        }
                    },
                    new List<object> {
                        "v1",
                        "v6"
                    }
                };
            this.assertEqual(this.dvvset.ids(a), new List<object> {
                    "a",
                    "b",
                    "c"
                });
            this.assertEqual(this.dvvset.ids(b), new List<object> {
                    "a",
                    "b",
                    "c"
                });
            this.assertEqual(this.dvvset.ids(c), new List<object> {
                    "a",
                    "b"
                });
            this.assertEqual(this.dvvset.values(a).OrderBy(p1 => p1).ToList(), new List<object> {
                    "v0",
                    "v1",
                    "v3",
                    "v5"
                });
            this.assertEqual(this.dvvset.values(b).OrderBy(p2 => p2).ToList(), new List<object> {
                    "v0",
                    "v3",
                    "v555"
                });
            this.assertEqual(this.dvvset.values(c).OrderBy(p3 => p3).ToList(), new List<object> {
                    "v1",
                    "v6"
                });
        }
    }
}
